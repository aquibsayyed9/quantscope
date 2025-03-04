using FixMessageAnalyzer.Core.Services;
using FixMessageAnalyzer.Core.Services;
using FixMessageAnalyzer.Data.Entities;
using QuickFix.Fields;
using System.ComponentModel.Design;
using System.Globalization;

namespace FixMessageAnalyzer.Services
{
    public interface IFixLogParsingService
    {
        FixMessage? ParseFixLogLine(string logLine, string fixVersion = null);
    }
}

namespace FixMessageAnalyzer.Services
{
    public class FixLogParsingService : IFixLogParsingService
    {
        private readonly ILogger<FixLogParsingService> _logger;
        private readonly IFixDictionaryService _dictionaryService;
        private readonly IFixMonitoringService _monitoringService;

        private static readonly string[] PossibleDelimiters = new[]
        {
            "\x01",    // SOH character
            "^A",      // Text representation of SOH
            "|",       // Sometimes used in logs
        };

        public FixLogParsingService(
            IFixMonitoringService monitoringService,
            IFixDictionaryService fieldMapper,
            ILogger<FixLogParsingService> logger)
        {
            _dictionaryService = fieldMapper;
            _monitoringService = monitoringService;
            _logger = logger;
        }

        public FixMessage? ParseFixLogLine(string logLine, string fixVersion = null)
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

                string msgType = fields.GetValueOrDefault("35", "Unknown");

                string messageVersion;
                if (!string.IsNullOrEmpty(fixVersion))
                {
                    messageVersion = fixVersion;
                }
                else
                {
                    // Auto-detect version
                    try
                    {
                        messageVersion = _dictionaryService.DetectVersion(fields);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to determine FIX version, using default");
                        messageVersion = "FIX.4.4"; // Default version as fallback
                    }
                }

                if (timestamp.HasValue && timestamp.Value.Kind != DateTimeKind.Utc)
                {
                    timestamp = DateTime.SpecifyKind(timestamp.Value, DateTimeKind.Utc);
                }

                // Fallback to current time if no timestamp found
                timestamp ??= DateTime.UtcNow;

                var message = new FixMessage
                {
                    Timestamp = timestamp.Value,
                    MsgType = msgType,
                    MsgTypeName = _dictionaryService.GetMessageTypeName(msgType, messageVersion),
                    SequenceNumber = int.TryParse(fields.GetValueOrDefault("34", "-1"), out int seqNum) ? seqNum : -1,
                    SenderCompID = fields.GetValueOrDefault("49", "Unknown"),
                    TargetCompID = fields.GetValueOrDefault("56", "Unknown"),
                    ExecType = fields.GetValueOrDefault("150"),
                    FixVersion = messageVersion,
                    Fields = fields,

                    // Extract common financial fields
                    ClOrdID = fields.GetValueOrDefault("11"),
                    OrderID = fields.GetValueOrDefault("37"),
                    ExecID = fields.GetValueOrDefault("17"),
                    Symbol = fields.GetValueOrDefault("55"),
                    SecurityType = fields.GetValueOrDefault("167"),
                    Side = fields.GetValueOrDefault("54"),
                    OrdType = fields.GetValueOrDefault("40"),
                    TimeInForce = fields.GetValueOrDefault("59"),
                    OrdStatus = fields.GetValueOrDefault("39"),
                    Account = fields.GetValueOrDefault("1")
                };

                // Try to parse numeric values
                if (fields.TryGetValue("44", out var priceStr) && decimal.TryParse(priceStr, out var price))
                {
                    message.Price = price;
                }

                if (fields.TryGetValue("38", out var qtyStr) && decimal.TryParse(qtyStr, out var qty))
                {
                    message.OrderQty = qty;
                }

                if (fields.TryGetValue("32", out var lastQtyStr) && decimal.TryParse(lastQtyStr, out var lastQty))
                {
                    message.LastQty = lastQty;
                }

                if (fields.TryGetValue("14", out var cumQtyStr) && decimal.TryParse(cumQtyStr, out var cumQty))
                {
                    message.CumQty = cumQty;
                }

                if (fields.TryGetValue("151", out var leavesQtyStr) && decimal.TryParse(leavesQtyStr, out var leavesQty))
                {
                    message.LeavesQty = leavesQty;
                }

                // Parse transaction time
                if (fields.TryGetValue("60", out var transactTimeStr))
                {
                    var transactTime = ParseFixTimestamp(transactTimeStr);

                    // Ensure TransactTime is UTC if it has a value
                    if (transactTime.HasValue && transactTime.Value.Kind != DateTimeKind.Utc)
                    {
                        message.TransactTime = DateTime.SpecifyKind(transactTime.Value, DateTimeKind.Utc);
                    }
                    else
                    {
                        message.TransactTime = transactTime;
                    }
                }

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