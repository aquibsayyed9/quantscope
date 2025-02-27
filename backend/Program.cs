using FixMessageAnalyzer.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.AspNetCore.Http.Features;
using FixMessageAnalyzer.Core.Services;
using Serilog;
using FixMessageAnalyzer.Api.Middleware;
using Serilog.Formatting.Json;


namespace FixMessageAnalyzer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            
            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen();

            // db connection
            var connectionString = builder.Configuration.GetConnectionString("pgsqlconnection");

            // Register all services
            builder.Services.AddApplicationServices(connectionString);
            //database service            
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.EnableDynamicJson(); // For JSONB serialization
            var dataSource = dataSourceBuilder.Build();

            builder.Services.AddDbContext<FixDbContext>(options =>
                   options.UseNpgsql(connectionString,
                       x => x.MigrationsHistoryTable("__EFMigrationsHistory", "fix")
                   )
               );

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy =>
                    {
                        policy.WithOrigins("*")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(new JsonFormatter()) // Log to Console
            .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day) // Log to File
            // can enable this if need all the logs in db, will increase overall write time
            //.WriteTo.PostgreSQL(builder.Configuration.GetConnectionString("pgsqlconnection"),
            //    tableName: "logs",
            //    needAutoCreateTable: true) // Log to PostgreSQL (Optional)
            .Enrich.FromLogContext()
            .CreateLogger();

            builder.Host.UseSerilog();

            var app = builder.Build();

            try
            {
                app.EnsureDatabaseMigrated();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database migration failed: {ex.Message}");
                throw;
            }

            app.UseSerilogRequestLogging();
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            if (!string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true"))
            {
                app.UseHttpsRedirection();
            }

            app.UseCors("AllowAll");
            app.UseAuthorization();

            app.MapControllers();            
            
            app.Run();
            
        }
    }
}
