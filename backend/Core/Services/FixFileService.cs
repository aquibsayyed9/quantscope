using FixMessageAnalyzer.Data;
using FixMessageAnalyzer.Services;
using FixMessageAnalyzer.Core.Services;
using FixMessageAnalyzer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FixMessageAnalyzer.Core.Services
{
    public interface IFixFileService
    {
        Task<string> ProcessFixLogFileAsync(Stream fileStream);
    }
}

namespace FixMessageAnalyzer.Core.Services
{
    public class FixFileService : IFixFileService
    {
        private readonly FixDbContext _dbContext;
        private readonly IFixLogParsingService _fixParser;
        private readonly ILogger<FixFileService> _logger;

        public FixFileService(FixDbContext dbContext, IFixLogParsingService fixParser, ILogger<FixFileService> logger)
        {
            _dbContext = dbContext;
            _fixParser = fixParser;
            _logger = logger;
        }

        public async Task<string> ProcessFixLogFileAsync(Stream fileStream)
        {
            string sessionId = Guid.NewGuid().ToString(); // Unique session ID
            _logger.LogInformation($"Processing FIX log file with sessionId: {sessionId}");

            using var reader = new StreamReader(fileStream);
            string line;
            var messages = new List<FixMessage>();

            while ((line = await reader.ReadLineAsync()) != null)
            {
                var fixMessage = _fixParser.ParseFixLogLine(line);
                if (fixMessage != null)
                {                    
                    messages.Add(fixMessage);
                }
            }

            await _dbContext.Messages.AddRangeAsync(messages);
            await _dbContext.SaveChangesAsync();

            return sessionId;
        }
    }
}
