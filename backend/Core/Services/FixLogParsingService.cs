using FixMessageAnalyzer.Core.Services;
using FixMessageAnalyzer.Core.Services;
using FixMessageAnalyzer.Data.Entities;
using System.Globalization;

namespace FixMessageAnalyzer.Services
{
    public interface IFixLogParsingService
    {
        FixMessage? ParseFixLogLine(string logLine);
    }
}

namespace FixMessageAnalyzer.Services
{
    public class FixLogParsingService : IFixLogParsingService
    {
        private readonly ILogger<FixLogParsingService> _logger;
        private readonly IFixFieldMapperService _fieldMapper;
        private readonly IFixMonitoringService _monitoringService;

        private static readonly string[] PossibleDelimiters = new[]
        {
            "\x01",    // SOH character
            "^A",      // Text representation of SOH
            "|",       // Sometimes used in logs
        };

        public FixLogParsingService(
            IFixMonitoringService monitoringService,
            IFixFieldMapperService fieldMapper,
            ILogger<FixLogParsingService> logger)
        {
            _fieldMapper = fieldMapper;
            _monitoringService = monitoringService;
            _logger = logger;
        }

        public FixMessage? ParseFixLogLine(string logLine)
        {
            try
            {
                // First find the actual FIX message content
                int fixMessageStart = FindFixMessageStart(logLine);
                if (fixMessageStart == -1)
                {
                    _logger.LogWarning("No valid FIX message found in line: {LogLine}", logLine);
                    return null;
                }

                // Extract potential timestamp from the prefix if it exists
                DateTime? timestamp = null;
                if (fixMessageStart > 0)
                {
                    string prefix = logLine.Substring(0, fixMessageStart).Trim(' ', '-', '[', ']');
                    timestamp = TryParseTimestamp(prefix);
                }

                string messageContent = logLine.Substring(fixMessageStart);

                // Replace any field delimiter variants with standard SOH
                foreach (var delimiter in PossibleDelimiters)
                {
                    messageContent = messageContent.Replace(delimiter, "\x01");
                }

                var fields = ParseFields(messageContent);

                if (!fields.Any())
                {
                    _logger.LogWarning("No valid fields found in message: {Content}", messageContent);
                    return null;
                }

                // If no external timestamp found, try to use SendingTime (tag 52) from FIX message
                if (!timestamp.HasValue && fields.TryGetValue("52", out string? sendingTime))
                {
                    timestamp = ParseFixTimestamp(sendingTime);
                }

                // Fallback to current time if no timestamp found
                timestamp ??= DateTime.UtcNow;

                var message = new FixMessage
                {
                    Timestamp = timestamp.Value,
                    MsgType = fields.GetValueOrDefault("35", "Unknown"),
                    SequenceNumber = int.TryParse(fields.GetValueOrDefault("34", "-1"), out int seqNum) ? seqNum : -1,
                    SenderCompID = fields.GetValueOrDefault("49", "Unknown"),
                    TargetCompID = fields.GetValueOrDefault("56", "Unknown"),
                    ExecType = fields.GetValueOrDefault("150"),
                    Fields = fields
                };

                _logger.LogDebug(
                    "Parsed message: Type={MsgType}, SeqNum={SeqNum}, Sender={Sender}, Target={Target}, Fields={FieldCount}",
                    message.MsgType,
                    message.SequenceNumber,
                    message.SenderCompID,
                    message.TargetCompID,
                    fields.Count);

                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing FIX message: {Line}", logLine);
                return null;
            }
        }

        private int FindFixMessageStart(string line)
        {
            // Look for FIX or FIXT message start
            int index = line.IndexOf("8=FIX");
            if (index == -1)
            {
                index = line.IndexOf("8=FIXT");
            }
            return index;
        }

        private DateTime? TryParseTimestamp(string text)
        {
            // Common timestamp formats
            string[] formats = new[]
            {
            "yyyyMMdd HH:mm:ss.fff",
            "yyyy-MM-dd HH:mm:ss.fff",
            "yyyyMMdd HH:mm:ss",
            "yyyy-MM-dd HH:mm:ss",
            "HH:mm:ss.fff",
            "HH:mm:ss"
        };

            if (DateTime.TryParseExact(text.Trim(), formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime result))
            {
                return result;
            }

            return null;
        }

        private DateTime? ParseFixTimestamp(string timestamp)
        {
            // FIX timestamp formats
            string[] formats = new[]
            {
            "yyyyMMdd-HH:mm:ss.fff",
            "yyyyMMdd-HH:mm:ss"
        };

            if (DateTime.TryParseExact(timestamp, formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime result))
            {
                return result;
            }

            return null;
        }

        private Dictionary<string, string> ParseFields(string messageContent)
        {
            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var field in messageContent.Split('\x01', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = field.Split('=', 2);
                if (parts.Length == 2)
                {
                    string tag = parts[0].Trim();
                    string value = parts[1].Trim();
                    fields[tag] = value;
                }
            }

            return fields;
        }
    }
}