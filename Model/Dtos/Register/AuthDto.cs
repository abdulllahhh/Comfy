using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dtos.Register
{
    public class AuthDto
    {
        public string Message { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresOn { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
        public bool IsAccountLocked { get; set; }
        public DateTime? LockoutEnd { get; set; }

        public int Credits { get; set; }
        public bool EmailVerificationRequired { get; set; }

    }
}
