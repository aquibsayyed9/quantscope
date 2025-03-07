using FixMessageAnalyzer.Data;
using FixMessageAnalyzer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using QuickFix.Fields;

namespace FixMessageAnalyzer.Services
{
    public interface IFixMessageService
    {
        Task<List<FixMessage>> GetMessagesAsync(int userId, string[]? msgTypes, string? orderId, DateTime? startTime, DateTime? endTime, int page = 1, 
            int pageSize = 100, bool skipHeartbeats = false);
    }
}

namespace FixMessageAnalyzer.Services
{
    public class FixMessageService : IFixMessageService
    {
        private readonly FixDbContext _dbContext;

        public FixMessageService(FixDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<FixMessage>> GetMessagesAsync(
        int userId,
        string[]? msgTypes,
        string? orderId,
        DateTime? startTime,
        DateTime? endTime,
        int page = 1,
        int pageSize = 100,
        bool skipHeartbeats = false)
        {
            // Validate parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 100;
            if (pageSize > 1000) pageSize = 1000; // Set a reasonable maximum

            var query = _dbContext.Messages.AsQueryable();
            query = query.Where(m => m.UserId == userId);

            // If no filters are applied, get latest messages
            if ((msgTypes == null || !msgTypes.Any()) &&
                string.IsNullOrEmpty(orderId) &&
                !startTime.HasValue &&
                !endTime.HasValue)
            {
                return await query
                    .OrderByDescending(m => m.SequenceNumber)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }

            // Apply filters
            if (msgTypes != null && msgTypes.Any())
                query = query.Where(m => msgTypes.Contains(m.MsgType));

            if (startTime.HasValue)
                query = query.Where(m => m.Timestamp >= startTime.Value);

            if (endTime.HasValue)
                query = query.Where(m => m.Timestamp <= endTime.Value);
            if (skipHeartbeats)
                query = query.Where(m => m.MsgType != "0" && m.MsgType != "1");

            //if (!string.IsNullOrEmpty(orderId))
            //    query = query.Where(m => EF.Functions.JsonContains(
            //        m.Fields,
            //        $"{{\"37\": \"{orderId}\"}}"
            //    ));
            if (!string.IsNullOrEmpty(orderId))
                query = query.Where(m => m.OrderID == orderId);

            // Calculate skip for pagination
            var skip = (page - 1) * pageSize;

            return await query
                .OrderByDescending(m => m.SequenceNumber)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }
    } 
}