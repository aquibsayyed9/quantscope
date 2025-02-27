using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace FixMessageAnalyzer.Data.DTOs
{
    public class ConnectorDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsActive { get; set; }
        public JsonDocument Configuration { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public DateTime? LastConnectedAt { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class CreateConnectorDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public JsonDocument Configuration { get; set; }

        public bool IsActive { get; set; } = false;
    }

    public class UpdateConnectorDto
    {
        public string Name { get; set; }
        public JsonDocument Configuration { get; set; }
        public bool? IsActive { get; set; }
    }
}