using FixMessageAnalyzer.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using FixMessageAnalyzer.Core.Services;
using Serilog;
using FixMessageAnalyzer.Api.Middleware;
using Serilog.Formatting.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FixMessageAnalyzer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Configure Serilog first, before any host building
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(new JsonFormatter())
                .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
                .Enrich.FromLogContext()
                .CreateLogger();

            try
            {
                Log.Information("Starting web application");

                var builder = WebApplication.CreateBuilder(args);

                // Configure Serilog for the host
                builder.Host.UseSerilog(Log.Logger);

                // Add services to the container
                builder.Services.AddControllers();

                builder.Services.AddOpenApi();
                builder.Services.AddSwaggerGen();

                // Database connection
                var connectionString = builder.Configuration.GetConnectionString("pgsqlconnection");

                // Register all services
                builder.Services.AddApplicationServices(connectionString);

                // Database service setup
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
                dataSourceBuilder.EnableDynamicJson(); // For JSONB serialization
                var dataSource = dataSourceBuilder.Build();

                builder.Services.AddDbContext<FixDbContext>(options =>
                    options.UseNpgsql(connectionString,
                        x => x.MigrationsHistoryTable("__EFMigrationsHistory", "fix")
                    )
                );

                // CORS
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

                // JWT Authentication
                var jwtKey = builder.Configuration["JWT:Secret"] ?? "your_default_secret_key_at_least_16_chars_long";
                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    };
                });

                var app = builder.Build();

                try
                {
                    app.EnsureDatabaseMigrated();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Database migration failed");
                    throw;
                }

                app.UseSerilogRequestLogging();
                app.UseMiddleware<ExceptionHandlingMiddleware>();

                // Configure the HTTP request pipeline
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
                app.UseAuthentication(); // Authentication must come BEFORE Authorization
                app.UseAuthorization();
                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application startup failed");
            }
            finally
            {
                // Ensure to flush and close the logger
                Log.CloseAndFlush();
            }
        }
    }
}