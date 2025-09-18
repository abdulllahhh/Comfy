using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model.Dtos.Register;
using Model.Entities;
using Model.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class EmailValidationService : IEmailValidationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        //private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly ILogger<EmailValidationService> _logger;

        // Common disposable email domains
        private readonly HashSet<string> _disposableEmailDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "10minutemail.com", "guerrillamail.com", "mailinator.com", "tempmail.org",
        "yopmail.com", "throwaway.email", "fakeinbox.com", "maildrop.cc",
        "temp-mail.org", "getnada.com", "sharklasers.com", "grr.la",
        "guerrillamailblock.com", "pokemail.net", "spam4.me", "bccto.me"
    };

        // Common valid email domains (for basic validation)
        private readonly HashSet<string> _commonValidDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "gmail.com", "yahoo.com", "hotmail.com", "outlook.com", "aol.com",
        "icloud.com", "protonmail.com", "live.com", "msn.com", "mail.com"
    };

        public EmailValidationService(
            UserManager<ApplicationUser> userManager,
        //    IEmailService emailService,
            IConfiguration config,
            ILogger<EmailValidationService> logger)
        {
            _userManager = userManager;
        //    _emailService = emailService;
            _config = config;
            _logger = logger;
        }

        public async Task<EmailValidationResult> ValidateEmailAsync(string email)
        {
            var result = new EmailValidationResult { Email = email };

            // 1. Basic format validation
            if (!IsValidEmailFormat(email))
            {
                result.IsValid = false;
                result.Errors.Add("Invalid email format");
                return result;
            }

            // 2. Check for disposable email domains
            var domain = GetDomainFromEmail(email);
            if (_disposableEmailDomains.Contains(domain))
            {
                result.IsValid = false;
                result.Errors.Add("Disposable email addresses are not allowed");
                return result;
            }

            // 3. Basic domain validation
            if (!await IsValidDomainAsync(domain))
            {
                result.IsValid = false;
                result.Errors.Add("Email domain does not exist or cannot receive emails");
                return result;
            }

            result.IsValid = true;
            return result;
        }

        private bool IsValidEmailFormat(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Use .NET's built-in email validation
                var addr = new MailAddress(email);
                return addr.Address == email && email.Contains('@') && !email.StartsWith('@') && !email.EndsWith('@');
            }
            catch
            {
                return false;
            }
        }

        private string GetDomainFromEmail(string email)
        {
            var atIndex = email.LastIndexOf('@');
            return atIndex > 0 ? email.Substring(atIndex + 1) : string.Empty;
        }

        private async Task<bool> IsValidDomainAsync(string domain)
        {
            // If it's a common domain, skip DNS check
            if (_commonValidDomains.Contains(domain))
                return true;

            try
            {
                // Simple DNS lookup
                var hostEntry = await Dns.GetHostEntryAsync(domain);
                return hostEntry.AddressList.Any();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("DNS lookup failed for domain {Domain}: {Error}", domain, ex.Message);
                return false;
            }
        }

        //public async Task<bool> SendVerificationEmailAsync(string email, string username, string token)
        //{
        //    try
        //    {
        //        var verificationLink = $"{_config["Frontend:Url"]}/verify-email?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";

        //        var subject = "Verify Your Email Address - AI Model ABI";
        //        var body = $@"
        //        <div style='max-width: 600px; margin: 0 auto; font-family: Arial, sans-serif;'>
        //            <h2 style='color: #333;'>Welcome to AI Model ABI, {username}!</h2>
        //            <p>Thank you for registering. Please verify your email address by clicking the button below:</p>
                    
        //            <div style='text-align: center; margin: 30px 0;'>
        //                <a href='{verificationLink}' 
        //                   style='background-color: #4CAF50; color: white; padding: 15px 30px; 
        //                          text-decoration: none; border-radius: 5px; display: inline-block;'>
        //                    Verify Email Address
        //                </a>
        //            </div>
                    
        //            <p><strong>This link will expire in 24 hours.</strong></p>
                    
        //            <p>If the button doesn't work, copy and paste this link into your browser:</p>
        //            <p><a href='{verificationLink}'>{verificationLink}</a></p>
                    
        //            <hr style='margin: 30px 0; border: 1px solid #eee;'>
        //            <p style='color: #666; font-size: 12px;'>
        //                If you didn't create an account with AI Model ABI, you can safely ignore this email.
        //            </p>
        //        </div>
        //    ";

        //        return await _emailService.SendEmailAsync(email, subject, body);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Failed to send verification email to {Email}: {Error}", email, ex.Message);
        //        return false;
        //    }
        //}

        public async Task<bool> VerifyEmailTokenAsync(string email, string token)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return false;

                var result = await _userManager.ConfirmEmailAsync(user, token);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError("Email verification failed for {Email}: {Error}", email, ex.Message);
                return false;
            }
        }
    }
}
