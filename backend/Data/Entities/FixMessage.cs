using System.ComponentModel.DataAnnotations.Schema;

namespace FixMessageAnalyzer.Data.Entities
{
    [Table("messages", Schema = "fix")]
    public class FixMessage
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string? SessionId { get; set; }
        public string MsgType { get; set; } = "Unknown";
        public string? MsgTypeName { get; set; } // Human-readable message type
        public int SequenceNumber { get; set; }
        public string SenderCompID { get; set; } = "Unknown";
        public string TargetCompID { get; set; } = "Unknown";

        // FIX Version information
        public string FixVersion { get; set; } = "Unknown";

        // Financial data fields (commonly queried)
        public string? ClOrdID { get; set; }              // Client Order ID (Tag 11)
        public string? OrderID { get; set; }              // Order ID (Tag 37)
        public string? ExecID { get; set; }               // Execution ID (Tag 17)
        public string? Symbol { get; set; }               // Instrument Symbol (Tag 55)
        public string? SecurityType { get; set; }         // Security Type (Tag 167)
        public decimal? Price { get; set; }               // Price (Tag 44)
        public decimal? OrderQty { get; set; }            // Order Quantity (Tag 38)
        public decimal? LastQty { get; set; }             // Last Quantity (Tag 32)
        public decimal? CumQty { get; set; }              // Cumulative Quantity (Tag 14)
        public decimal? LeavesQty { get; set; }           // Leaves Quantity (Tag 151)
        public string? Side { get; set; }                 // Side (Tag 54): 1=Buy, 2=Sell
        public string? OrdType { get; set; }              // Order Type (Tag 40)
        public string? TimeInForce { get; set; }          // Time In Force (Tag 59)
        public string? OrdStatus { get; set; }            // Order Status (Tag 39)
        public string? ExecType { get; set; }             // Execution Type (Tag 150)
        public string? Account { get; set; }              // Account (Tag 1)
        public DateTime? TransactTime { get; set; }       // Transaction Time (Tag 60)

        // Validation status
        public bool IsValid { get; set; } = true;
        public List<string> ValidationErrors { get; set; } = new();

        // All fields stored in JSONB
        public Dictionary<string, string> Fields { get; set; } = new();

        // Metadata 
        public DateTime CreatedAt { get; set; }

        // User entity
        public int? UserId { get; set; }
        public virtual User User { get; set; }
    }
}
