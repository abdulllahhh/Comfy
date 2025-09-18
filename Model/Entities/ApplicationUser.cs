using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entities
{
    public class ApplicationUser : IdentityUser
    {
        
        public int Credits { get; set; } = 10; // default free credits
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
        public DateTime? LastLoginDate { get; set; }

        public ICollection<Payment> Payments { get; set; }

    }
}
