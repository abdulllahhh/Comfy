using Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Interface
{
    public interface IEmailValidationService
    {
        Task<EmailValidationResult> ValidateEmailAsync(string email);
        //Task<bool> SendVerificationEmailAsync(string email, string username, string token);
        Task<bool> VerifyEmailTokenAsync(string email, string token);
    }
}
