using FixMessageAnalyzer.Data.Entities;
using FixMessageAnalyzer.Data;
using FixMessageAnalyzer.Core.Workers;
using FixMessageAnalyzer.Data.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FixMessageAnalyzer.Services
{
    public interface IConnectorService
    {
        Task<IEnumerable<ConnectorDto>> GetConnectorsAsync(string type = null);
        Task<ConnectorDto> GetConnectorAsync(int id);
        Task<ConnectorDto> CreateConnectorAsync(CreateConnectorDto request);
        Task UpdateConnectorAsync(int id, UpdateConnectorDto request);
        Task DeleteConnectorAsync(int id);
        Task ToggleConnectorAsync(int id);
    }
    public class ConnectorHostedService : BackgroundService, IConnectorService
    {
        private readonly ILogger<ConnectorHostedService> _logger;
        private readonly IServiceProvider _services;
        private readonly Dictionary<int, IConnectorWorker> _activeWorkers;

        public ConnectorHostedService(
            ILogger<ConnectorHostedService> logger,
            IServiceProvider services)
        {
            _logger = logger;
            _services = services;
            _activeWorkers = new Dictionary<int, IConnectorWorker>();
        }

        public async Task<IEnumerable<ConnectorDto>> GetConnectorsAsync(string type = null)
        {
            using var scope = _services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FixDbContext>();

            IQueryable<Connector> query = dbContext.Connectors;
            if (!string.IsNullOrEmpty(type) && Enum.TryParse<ConnectorType>(type, out var connectorType))
            {
                query = query.Where(c => c.Type == connectorType);
            }

            var connectors = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return connectors.Select(MapToDto);
        }

        public async Task<ConnectorDto> GetConnectorAsync(int id)
        {
            using var scope = _services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FixDbContext>();

            var connector = await dbContext.Connectors.FindAsync(id);
            return connector != null ? MapToDto(connector) : null;
        }

        public async Task<ConnectorDto> CreateConnectorAsync(CreateConnectorDto request)
        {
            using var scope = _services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FixDbContext>();

            if (!Enum.TryParse<ConnectorType>(request.Type, out var connectorType))
            {
                throw new ArgumentException($"Invalid connector type: {request.Type}");
            }

            var connector = new Connector
            {
                Name = request.Name,
                Type = connectorType,
                IsActive = request.IsActive,
                Configuration = request.Configuration,
                CreatedAt = DateTime.UtcNow,
                Status = "Created"
            };

            dbContext.Connectors.Add(connector);
            await dbContext.SaveChangesAsync();

            return MapToDto(connector);
        }

        public async Task UpdateConnectorAsync(int id, UpdateConnectorDto request)
        {
            using var scope = _services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FixDbContext>();

            var connector = await dbContext.Connectors.FindAsync(id);
            if (connector == null)
            {
                throw new KeyNotFoundException($"Connector {id} not found");
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                connector.Name = request.Name;
            }

            if (request.Configuration != null)
            {
                connector.Configuration = request.Configuration;
            }

            if (request.IsActive.HasValue)
            {
                connector.IsActive = request.IsActive.Value;
                connector.Status = request.IsActive.Value ? "Starting" : "Stopped";
            }

            connector.LastModifiedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteConnectorAsync(int id)
        {
            using var scope = _services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FixDbContext>();

            var connector = await dbContext.Connectors.FindAsync(id);
            if (connector == null)
            {
                throw new KeyNotFoundException($"Connector {id} not found");
            }

            if (_activeWorkers.TryGetValue(connector.Id, out var worker))
            {
                await worker.StopAsync(default);
                _activeWorkers.Remove(connector.Id);
            }

            dbContext.Connectors.Remove(connector);
            await dbContext.SaveChangesAsync();
        }

        public async Task ToggleConnectorAsync(int id)
        {
            using var scope = _services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FixDbContext>();

            var connector = await dbContext.Connectors.FindAsync(id);
            if (connector == null)
            {
                throw new KeyNotFoundException($"Connector {id} not found");
            }

            connector.IsActive = !connector.IsActive;
            connector.LastModifiedAt = DateTime.UtcNow;
            connector.Status = connector.IsActive ? "Starting" : "Stopped";

            await dbContext.SaveChangesAsync();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateWorkers(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating workers");
                }
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private async Task UpdateWorkers(CancellationToken stoppingToken)
        {
            using var scope = _services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FixDbContext>();

            var activeConnectors = await dbContext.Connectors
                .Where(c => c.IsActive)
                .ToListAsync(stoppingToken);

            // Stop workers for disabled connectors
            var connectorsToRemove = _activeWorkers.Keys
                .Except(activeConnectors.Select(c => c.Id))
                .ToList();

            foreach (var connectorId in connectorsToRemove)
            {
                if (_activeWorkers.TryGetValue(connectorId, out var worker))
                {
                    await worker.StopAsync(stoppingToken);
                    _activeWorkers.Remove(connectorId);
                }
            }

            // Start or update workers for active connectors
            foreach (var connector in activeConnectors)
            {
                if (!_activeWorkers.TryGetValue(connector.Id, out var worker))
                {
                    worker = CreateWorker(connector);
                    if (worker != null)
                    {
                        _activeWorkers[connector.Id] = worker;
                        await worker.StartAsync(stoppingToken);
                    }
                }
                else
                {
                    await worker.UpdateConfigurationAsync(connector, stoppingToken);
                }
            }
        }

        private IConnectorWorker CreateWorker(Connector connector)
        {
            return connector.Type switch
            {
                ConnectorType.FileUpload => new FileUploadWorker(_logger, connector, _services),
                //ConnectorType.Nats => new NatsWorker(_logger, connector, _services),
                //ConnectorType.Kafka => new KafkaWorker(_logger, connector, _services),
                //ConnectorType.FixGateway => new FixGatewayWorker(_logger, connector, _services),
                _ => null
            };
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var worker in _activeWorkers.Values)
            {
                await worker.StopAsync(cancellationToken);
            }
            await base.StopAsync(cancellationToken);
        }

        private static ConnectorDto MapToDto(Connector entity)
        {
            return new ConnectorDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Type = entity.Type.ToString(),
                IsActive = entity.IsActive,
                Configuration = entity.Configuration,
                CreatedAt = entity.CreatedAt,
                LastModifiedAt = entity.LastModifiedAt,
                LastConnectedAt = entity.LastConnectedAt,
                Status = entity.Status,
                ErrorMessage = entity.ErrorMessage
            };
        }
    }
}