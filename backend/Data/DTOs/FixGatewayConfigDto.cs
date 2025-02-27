namespace FixMessageAnalyzer.Data.DTOs
{
    public class FixGatewayConfigDto
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string SenderCompId { get; set; }
        public string TargetCompId { get; set; }
        public Dictionary<string, string> SessionSettings { get; set; }
    }
}
