using FixMessageAnalyzer.Data;
using FixMessageAnalyzer.Services;
using FixMessageAnalyzer.Core.Services;
using FixMessageAnalyzer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FixMessageAnalyzer.Core.Services
{
    public interface IFixFileService
    {
        Task<string> ProcessFixLogFileAsync(Stream fileStream, string fixVersion = null);
    }
}

namespace FixMessageAnalyzer.Core.Services
{
    public class FixFileService : IFixFileService
    {
        private readonly FixDbContext _dbContext;
        private readonly IFixLogParsingService _fixParser;
        private readonly IFixValidationService _fixValidator;
        private readonly ILogger<FixFileService> _logger;

        public FixFileService(
            FixDbContext dbContext,
            IFixLogParsingService fixParser,
            IFixValidationService fixValidator,
            ILogger<FixFileService> logger)
        {
            _dbContext = dbContext;
            _fixParser = fixParser;
            _fixValidator = fixValidator;
            _logger = logger;
        }

        public async Task<string> ProcessFixLogFileAsync(Stream fileStream, string fixVersion = null)
        {
            string sessionId = Guid.NewGuid().ToString();
            _logger.LogInformation($"Processing FIX log file with sessionId: {sessionId}, version: {fixVersion ?? "auto-detect"}");

            using var reader = new StreamReader(fileStream);
            string line;
            var messages = new List<FixMessage>();

            while ((line = await reader.ReadLineAsync()) != null)
            {
                var fixMessage = _fixParser.ParseFixLogLine(line, fixVersion);
                if (fixMessage != null)
                {
                    fixMessage.SessionId = sessionId;

                    // Validate the message
                    var validationResult = _fixValidator.ValidateMessage(fixMessage);
                    fixMessage.IsValid = validationResult.IsValid;
                    fixMessage.ValidationErrors = validationResult.Errors;

                    messages.Add(fixMessage);
                }
            }

            await _dbContext.Messages.AddRangeAsync(messages);
            await _dbContext.SaveChangesAsync();

            return sessionId;
        }
    }
}
