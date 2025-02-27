using System.Text.Json;
using FixMessageAnalyzer.Data;
using FixMessageAnalyzer.Data.Entities;
using FixMessageAnalyzer.Services;
using FixMessageAnalyzer.Data.DTOs;
using FixMessageAnalyzer.Data.Entities;

namespace FixMessageAnalyzer.Core.Workers
{
    public interface IConnectorWorker
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
        Task UpdateConfigurationAsync(Connector connector, CancellationToken cancellationToken);
    }

    public class FileUploadWorker : IConnectorWorker
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _services;
        private Connector _connector;
        private FileSystemWatcher _watcher;
        private HashSet<string> _processedFiles;
        private readonly object _lock = new object();
        private CancellationTokenSource _cts;

        public FileUploadWorker(ILogger logger, Connector connector, IServiceProvider services)
        {
            _logger = logger;
            _connector = connector;
            _services = services;
            _processedFiles = new HashSet<string>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var config = JsonSerializer.Deserialize<FileUploadConfigDto>(_connector.Configuration);
            _logger.LogInformation("Starting FileUploadWorker with config: Directory={Directory}, Pattern={Pattern}, SubDirs={SubDirs}",
        config?.Directory, config?.Pattern, config?.ProcessSubDirectories);

            if (config == null)
            {
                throw new InvalidOperationException("Invalid file upload configuration");
            }

            // Start directory monitoring
            _watcher = new FileSystemWatcher
            {
                Path = config.Directory,
                Filter = config.Pattern,
                EnableRaisingEvents = true,
                IncludeSubdirectories = config.ProcessSubDirectories
            };

            _watcher.Created += OnFileCreated;
            _watcher.Changed += OnFileChanged;

            _watcher.Error += (sender, error) =>
            {
                _logger.LogError(error.GetException(), "FileSystemWatcher error");
            };

            // Process existing files
            Task.Run(() => ProcessExistingFiles(config), _cts.Token);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts?.Cancel();
            _watcher?.Dispose();
            return Task.CompletedTask;
        }

        public Task UpdateConfigurationAsync(Connector connector, CancellationToken cancellationToken)
        {
            _connector = connector;
            var config = JsonSerializer.Deserialize<FileUploadConfigDto>(_connector.Configuration);

            if (_watcher != null && config != null)
            {
                _watcher.Path = config.Directory;
                _watcher.Filter = config.Pattern;
                _watcher.IncludeSubdirectories = config.ProcessSubDirectories;
            }

            return Task.CompletedTask;
        }

        private async Task ProcessExistingFiles(FileUploadConfigDto config)
        {
            try
            {
                var files = Directory.GetFiles(
                    config.Directory,
                    config.Pattern,
                    config.ProcessSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
                );

                foreach (var file in files.OrderBy(f => File.GetCreationTime(f)))
                {
                    await ProcessFileAsync(file);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing existing files in directory {Directory}", config.Directory);
                UpdateConnectorStatus("Error", ex.Message);
            }
        }

        private async void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation("File created event triggered: {Path}", e.FullPath);
            await ProcessFileAsync(e.FullPath);
        }

        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation("File changed event triggered: {Path}", e.FullPath);
            await ProcessFileAsync(e.FullPath);
        }

        private async Task ProcessFileAsync(string filePath)
        {
            if (_cts.Token.IsCancellationRequested)
                return;

            lock (_lock)
            {
                if (_processedFiles.Contains(filePath))
                    return;

                _processedFiles.Add(filePath);
            }

            try
            {
                using var scope = _services.CreateScope();
                var parsingService = scope.ServiceProvider.GetRequiredService<IFixLogParsingService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<FixDbContext>();

                // Wait for file to be completely written
                await WaitForFile(filePath);

                UpdateConnectorStatus("Processing", null);

                // Read and parse file line by line
                var messages = new List<FixMessage>();
                foreach (var line in await File.ReadAllLinesAsync(filePath, _cts.Token))
                {
                    var message = parsingService.ParseFixLogLine(line);
                    if (message != null)
                    {
                        messages.Add(message);
                    }
                }

                // Store messages in batches
                if (messages.Any())
                {
                    const int batchSize = 1000;
                    for (int i = 0; i < messages.Count; i += batchSize)
                    {
                        var batch = messages.Skip(i).Take(batchSize).ToList();
                        dbContext.Messages.AddRange(batch);
                        await dbContext.SaveChangesAsync(_cts.Token);
                    }
                }

                UpdateConnectorStatus("Active", null);
                _logger.LogInformation("Successfully processed file: {FilePath}, Messages: {Count}",
                    filePath, messages.Count);
            }
            catch (Exception ex)
            {
                UpdateConnectorStatus("Error", ex.Message);
                _logger.LogError(ex, "Error processing file: {FilePath}", filePath);
            }
        }

        private async Task WaitForFile(string filePath)
        {
            var attempts = 0;
            while (attempts < 5)
            {
                try
                {
                    using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                    return;
                }
                catch (IOException)
                {
                    attempts++;
                    await Task.Delay(1000, _cts.Token);
                }
            }
            throw new TimeoutException($"File {filePath} is locked for too long");
        }

        private async void UpdateConnectorStatus(string status, string error)
        {
            try
            {
                using var scope = _services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<FixDbContext>();

                var connector = await dbContext.Connectors.FindAsync(_connector.Id);
                if (connector != null)
                {
                    connector.Status = status;
                    connector.ErrorMessage = error;
                    connector.LastConnectedAt = DateTime.UtcNow;
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating connector status");
            }
        }
    }
}