namespace FixMessageAnalyzer.Data.DTOs
{
    public class LastMessageTimestampDto
    {
        public string SessionKey { get; set; }
        public DateTime LastTimestamp { get; set; }
    }
}
