using System.Text.Json;

namespace FixMessageAnalyzer.Data.Entities
{
    public class Connector
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ConnectorType Type { get; set; }
        public bool IsActive { get; set; }
        public JsonDocument Configuration { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public DateTime? LastConnectedAt { get; set; }
        public string Status { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public enum ConnectorType
    {
        FileUpload,
        Nats,
        Kafka,
        FixGateway
    }
}