using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FixMessageAnalyzer.Data;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FixMessageAnalyzer.Services
{
    public interface IFixAnalyticsService
    {
        Task<Dictionary<string, int>> GetSymbolDistributionAsync();
        Task<Dictionary<string, int>> GetSymbolDistributionLinqAsync();
    }
}

namespace FixMessageAnalyzer.Services
{
    public class SymbolCount
    {
        public string Symbol { get; set; }
        public int MessageCount { get; set; }
    }
    public class FixAnalyticsService : IFixAnalyticsService
    {
        private readonly FixDbContext _dbContext;

        public FixAnalyticsService(FixDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Dictionary<string, int>> GetSymbolDistributionAsync()
        {
            var query = @"
        SELECT 
            m.fields->>'55' as symbol,
            COUNT(*) as message_count
        FROM fix.messages m
        WHERE 
            m.fields ? '55'
            AND m.msg_type = '8'
        GROUP BY m.fields->>'55'";

            // Create a result class to map the query results


            var results = await _dbContext.Set<SymbolCount>()
                .FromSqlRaw(query)
                .ToListAsync();

            return results.ToDictionary(
                x => x.Symbol ?? "Unknown",
                x => x.MessageCount,
                StringComparer.OrdinalIgnoreCase
            );
        }

        // Alternative approach using EF Core and LINQ
        public async Task<Dictionary<string, int>> GetSymbolDistributionLinqAsync()
        {
            return await _dbContext.Messages
                .Where(m => m.MsgType == "8")
                .Where(m => EF.Functions.JsonExists(m.Fields, "55"))
                .Select(m => new
                {
                    Symbol = EF.Functions.JsonContains(m.Fields, "55").ToString(),
                    MsgType = m.MsgType
                })
                .GroupBy(x => x.Symbol)
                .Select(g => new { Symbol = g.Key, Count = g.Count() })
                .ToDictionaryAsync(
                    x => x.Symbol ?? "Unknown",
                    x => x.Count,
                    StringComparer.OrdinalIgnoreCase
                );
        }
    }
}
