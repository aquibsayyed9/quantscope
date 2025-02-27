namespace FixMessageAnalyzer.Data.DTOs
{
    public class KafkaConfigDto
    {
        public string BootstrapServers { get; set; }
        public string Topic { get; set; }
        public string GroupId { get; set; }
        public Dictionary<string, string> SecurityConfig { get; set; }
    }
}
