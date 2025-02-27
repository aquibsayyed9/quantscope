namespace FixMessageAnalyzer.Data.DTOs
{
    public class NatsConfigDto
    {
        public string Url { get; set; }
        public string Subject { get; set; }
        public string QueueGroup { get; set; }
        public Dictionary<string, string> Credentials { get; set; }
    }
}
