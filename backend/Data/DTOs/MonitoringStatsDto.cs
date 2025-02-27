using FixMessageAnalyzer.Core.Services;

namespace FixMessageAnalyzer.Data.DTOs
{
    public class MonitoringStatsDto
    {
        public List<MessageRateDto> MessageRates { get; set; } = new();
        public List<LastMessageTimestampDto> LastMessageTimestamps { get; set; } = new();
        public MessageTypeStatsDto MessageTypes { get; set; } = new();
        public LatencyStatsDto LatencyStats { get; set; } = new();
        public SessionHealthDto SessionHealth { get; set; } = new();
        public int SequenceResets { get; set; }
        public int RejectedMessages { get; set; }
        public int ExchangeResets { get; set; }
        public int SessionMessages { get; set; }
    }
}
