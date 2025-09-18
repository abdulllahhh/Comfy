using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model.Entities;
using Model.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<EmailSettings> emailSettings, ILogger<SmtpEmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string htmlBody)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                    EnableSsl = _emailSettings.EnableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to send email to {Email}: {Error}", to, ex.Message);
                return false;
            }
        }

        public async Task SendAccountLockedEmailAsync(string email, string username)
        {
            var subject = "Account Security Alert - Account Locked";
            var body = $@"
            <h2>Account Locked</h2>
            <p>Hello {username},</p>
            <p>Your account has been temporarily locked due to multiple failed login attempts.</p>
            <p>Your account will be automatically unlocked in 15 minutes, or you can contact support.</p>
            <p>If this wasn't you, please change your password immediately after the lockout period.</p>
            <hr>
            <p>AI Model ABI Security Team</p>
        ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendAccountUnlockedEmailAsync(string email, string username)
        {
            var subject = "Account Unlocked";
            var body = $@"
            <h2>Account Unlocked</h2>
            <p>Hello {username},</p>
            <p>Your account has been unlocked and you can now log in normally.</p>
            <p>If you didn't request this unlock, please contact support immediately.</p>
            <hr>
            <p>AI Model ABI Security Team</p>
        ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendUnlockTokenEmailAsync(string email, string username, string token)
        {
            var unlockLink = $"https://yourapp.com/unlock-account?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

            var subject = "Unlock Your Account";
            var body = $@"
            <h2>Account Unlock Request</h2>
            <p>Hello {username},</p>
            <p>Click the link below to unlock your account:</p>
            <p><a href='{unlockLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Unlock Account</a></p>
            <p>This link will expire in 1 hour.</p>
            <hr>
            <p>AI Model ABI Security Team</p>
        ";

            await SendEmailAsync(email, subject, body);
        }
    }
}
