using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FixMessageAnalyzer.Data
{
    public static class DatabaseMigrationExtensions
    {
        public static void EnsureDatabaseMigrated(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<FixDbContext>();
                var logger = services.GetRequiredService<ILogger<FixDbContext>>();

                try
                {
                    // Comprehensive database connection and migration logging
                    logger.LogInformation("Starting database migration process...");

                    // Detailed connection check
                    try
                    {
                        bool canConnect = context.Database.CanConnect();
                        logger.LogInformation($"Database connection status: {canConnect}");
                    }
                    catch (Exception connectEx)
                    {
                        logger.LogError(connectEx, "Failed to check database connection");
                        throw;
                    }

                    // Ensure database is created
                    //logger.LogInformation("Attempting to ensure database is created...");
                    //context.Database.EnsureCreated();

                    // Check and apply migrations
                    var pendingMigrations = context.Database.GetPendingMigrations().ToList();

                    logger.LogInformation($"Pending migrations found: {pendingMigrations.Count}");
                    foreach (var migration in pendingMigrations)
                    {
                        logger.LogInformation($"Pending migration: {migration}");
                    }

                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation("Applying pending migrations...");
                        context.Database.Migrate();
                        logger.LogInformation("Migrations applied successfully.");
                    }
                    else
                    {
                        logger.LogInformation("No pending migrations to apply.");
                    }

                    // Verify schema and tables
                    VerifyDatabaseSchema(context, logger);
                }
                catch (PostgresException pgEx)
                {
                    logger.LogError(pgEx, $"PostgreSQL specific migration error. SQLState: {pgEx.SqlState}");
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Comprehensive error during database migration");
                    throw;
                }
            }
        }

        private static void VerifyDatabaseSchema(FixDbContext context, ILogger logger)
        {
            try
            {
                using (var connection = context.Database.GetDbConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT table_schema, table_name 
                        FROM information_schema.tables 
                        WHERE table_schema = 'fix'";

                    logger.LogInformation("Checking existing tables in 'fix' schema:");

                    using (var reader = command.ExecuteReader())
                    {
                        bool hasTables = false;
                        while (reader.Read())
                        {
                            hasTables = true;
                            logger.LogInformation($"Existing table: {reader["table_schema"]}.{reader["table_name"]}");
                        }

                        if (!hasTables)
                        {
                            logger.LogWarning("No tables found in 'fix' schema.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error verifying database schema");
            }
        }
    }
}