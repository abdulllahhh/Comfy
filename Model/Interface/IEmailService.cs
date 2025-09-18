using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Interface
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string htmlBody);
        Task SendAccountLockedEmailAsync(string email, string username);
        Task SendAccountUnlockedEmailAsync(string email, string username);
        Task SendUnlockTokenEmailAsync(string email, string username, string token);
    }
}
