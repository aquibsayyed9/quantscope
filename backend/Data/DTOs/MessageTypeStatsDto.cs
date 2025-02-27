namespace FixMessageAnalyzer.Data.DTOs
{
    public class MessageTypeStatsDto
    {
        public int NewOrders { get; set; }
        public int RestatedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public Dictionary<string, int> MessageTypeDistribution { get; set; } = new();
    }
}
