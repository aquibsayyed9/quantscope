using System.ComponentModel.DataAnnotations.Schema;

namespace FixMessageAnalyzer.Data.Entities
{
    [Table("messages", Schema = "fix")]
    public class FixMessage
    {
        public long Id { get; set; }

        private DateTime _timestamp;
        public DateTime Timestamp
        {
            get => _timestamp;
            set => _timestamp = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        public string MsgType { get; set; }
        public int SequenceNumber { get; set; }
        public string SenderCompID { get; set; }
        public string TargetCompID { get; set; }
        public string SessionId { get; set; }
        [Column(TypeName = "jsonb")]
        public Dictionary<string, string> Fields { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Ensure this is also UTC
        public string? ExecType { get; set; }
    }

}
