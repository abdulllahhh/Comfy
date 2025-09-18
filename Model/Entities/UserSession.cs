using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entities
{
    public class UserSession
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Foreign key to ApplicationUser
        public string RefreshToken { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
        // Navigation property
        public virtual ApplicationUser User { get; set; }
    }
}
