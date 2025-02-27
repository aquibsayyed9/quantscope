using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FixMessageAnalyzer.Data;
using Microsoft.EntityFrameworkCore;

namespace FixMessageAnalyzer.Core.Services
{
    public interface IFixSessionService
    {
        Task<object> GetSessionDetailsAsync(long sessionId);
    }
}

namespace FixMessageAnalyzer.Core.Services
{
    public class FixSessionService : IFixSessionService
    {
        private readonly FixDbContext _dbContext;

        public FixSessionService(FixDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        //public async Task<List<string>> GetAllSessionsAsync()
        //{
        //    return await _dbContext.Messages
        //        .Select(m => m.Id)
        //        .Distinct()
        //        .ToListAsync();
        //}

        public async Task<object> GetSessionDetailsAsync(long sessionId)
        {
            var messages = await _dbContext.Messages
                .Where(m => m.Id == sessionId)
                .ToListAsync();

            if (!messages.Any())
                return null;

            return new
            {
                SessionId = sessionId,
                Status = "Completed",
                MessageCount = messages.Count,
                StartTime = messages.Min(m => m.Timestamp),
                EndTime = messages.Max(m => m.Timestamp)
            };
        }
    }
}
