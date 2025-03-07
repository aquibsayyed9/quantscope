using System.Collections.Generic;

namespace FixMessageAnalyzer.Data.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }

        // Navigation property for the user's messages
        public virtual ICollection<FixMessage> Messages { get; set; }
    }
}