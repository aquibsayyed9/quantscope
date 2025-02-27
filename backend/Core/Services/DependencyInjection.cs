using FixMessageAnalyzer.Data;
using FixMessageAnalyzer.Services;
using Microsoft.EntityFrameworkCore;
using FixMessageAnalyzer.Core.Services;

namespace FixMessageAnalyzer.Core.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, string connectionString)
        {
            // Register Database Context
            services.AddDbContext<FixDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Register Services
            services.AddScoped<IFixFileService, FixFileService>();
            services.AddScoped<IFixSessionService, FixSessionService>();
            services.AddScoped<IFixMessageService, FixMessageService>();
            services.AddScoped<IFixOrderService, FixOrderService>();
            services.AddScoped<IFixAnalyticsService, FixAnalyticsService>();
            services.AddScoped<IFixLogParsingService, FixLogParsingService>();
            services.AddScoped<IFixMonitoringService, FixMonitoringService>();
            services.AddScoped<IFixFieldMapperService, FixFieldMapper>();
            //services.AddScoped<IConnectorService, ConnectorHostedService>();
            //services.AddHostedService<ConnectorHostedService>();

            return services;
        }
    }
}
