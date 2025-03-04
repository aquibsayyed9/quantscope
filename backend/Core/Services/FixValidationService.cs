using FixMessageAnalyzer.Data.Entities;

namespace FixMessageAnalyzer.Core.Services
{
    public interface IFixValidationService
    {
        ValidationResult ValidateMessage(FixMessage message);
    }

    public class ValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; } = new();

        public void AddError(string error)
        {
            Errors.Add(error);
        }
    }

    public class FixValidationService : IFixValidationService
    {
        private readonly IFixDictionaryService _dictionaryService;
        private readonly ILogger<FixValidationService> _logger;

        public FixValidationService(
            IFixDictionaryService dictionaryService,
            ILogger<FixValidationService> logger)
        {
            _dictionaryService = dictionaryService;
            _logger = logger;
        }

        public ValidationResult ValidateMessage(FixMessage message)
        {
            var result = new ValidationResult();
            var version = message.FixVersion;
            var msgType = message.MsgType;

            // Check if message type is recognized
            if (_dictionaryService.GetMessageTypeName(msgType, version) == $"Unknown ({msgType})")
            {
                result.AddError($"Unknown message type: {msgType} for version {version}");
            }

            // Check for required fields
            foreach (var field in message.Fields)
            {
                string tag = field.Key;
                string value = field.Value;

                // Validate enum values 
                if (!_dictionaryService.IsValidValue(tag, value, version))
                {
                    string fieldName = _dictionaryService.GetFieldName(tag, version);
                    result.AddError($"Invalid value '{value}' for field {fieldName} ({tag})");
                }
            }

            return result;
        }
    }
}
