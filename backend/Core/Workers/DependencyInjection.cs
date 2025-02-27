using FixMessageAnalyzer.Data;
using FixMessageAnalyzer.Services;
using Microsoft.EntityFrameworkCore;
using FixMessageAnalyzer.Core.Services;

namespace FixMessageAnalyzer.Core.Workers
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, string connectionString)
        {

            // Register Services
            //services.AddScoped<FileUploadWorker>();

            return services;
        }
    }
}
