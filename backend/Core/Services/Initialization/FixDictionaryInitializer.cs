namespace FixMessageAnalyzer.Core.Services
{
    public class FixDictionaryInitializer : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FixDictionaryInitializer> _logger;

        public FixDictionaryInitializer(
            IServiceProvider serviceProvider,
            ILogger<FixDictionaryInitializer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initializing FIX dictionaries");

            // Create a scope to resolve the scoped service
            using var scope = _serviceProvider.CreateScope();
            var dictionaryService = scope.ServiceProvider.GetRequiredService<IFixDictionaryService>();

            await dictionaryService.InitializeDictionariesAsync();

            _logger.LogInformation("FIX dictionaries initialized successfully");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
