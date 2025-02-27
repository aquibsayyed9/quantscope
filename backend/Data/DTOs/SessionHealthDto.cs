namespace FixMessageAnalyzer.Data.DTOs
{
    public class SessionHealthDto
    {
        public int TotalGaps { get; set; }
        public Dictionary<string, List<SequenceGap>> SequenceGaps { get; set; } = new();
        public double HealthScore { get; set; }
    }
    public class SequenceGap
    {
        public int ExpectedSeqNum { get; set; }
        public int ReceivedSeqNum { get; set; }
        public DateTime DetectedAt { get; set; }
    }
}
