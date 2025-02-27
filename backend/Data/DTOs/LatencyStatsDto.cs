namespace FixMessageAnalyzer.Data.DTOs
{
    public class LatencyStatsDto
    {
        public double AverageLatencyMs { get; set; }
        public double P95LatencyMs { get; set; }
        public double P99LatencyMs { get; set; }
    }
}
