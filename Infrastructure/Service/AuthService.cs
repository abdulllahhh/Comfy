using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Model.Dtos.Register;
using Model.Entities;
using Model.Helper;
using Model.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Service
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailValidationService _emailValidation;
        private readonly JWT _jwt;
        private readonly ILogger<AuthService> _logger;


        public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IEmailValidationService emailValidation,
        IOptions<JWT> jwt,
        ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailValidation = emailValidation;
            _jwt = jwt.Value;
            _logger = logger;
        }

        public async Task<AuthDto> RegisterAsync(RegisterDto model)
        {
            try
            {
                var emailValidation = await _emailValidation.ValidateEmailAsync(model.Email);
                if (!emailValidation.IsValid)
                {
                    var errors = string.Join(", ", emailValidation.Errors);
                    return new AuthDto { Message = $"Email validation failed: {errors}" };
                }


                if (await _userManager.FindByEmailAsync(model.Email) is not null)
                    return new AuthDto { Message = "Can't use this Email Address" };

                if (await _userManager.FindByNameAsync(model.Username) is not null)
                    return new AuthDto { Message = "Can't use this Username" };

                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    Credits = 10,
                    EmailConfirmed = true // Important: email not confirmed yet// add it when sending email
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Empty;

                    foreach (var error in result.Errors)
                        errors += $"{error.Description},";

                    return new AuthDto { Message = errors };
                }

                await _userManager.AddToRoleAsync(user, "User");

                var jwtSecurityToken = await CreateJwtToken(user);

                return new AuthDto
                {
                    Email = user.Email,
                    ExpiresOn = jwtSecurityToken.ValidTo,
                    IsAuthenticated = true,
                    Roles = new List<string> { "User" },
                    Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                    Username = user.UserName,
                    Message = "Registration successful!",
                    EmailVerificationRequired = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for {Email}", model.Email);
                return new AuthDto { Message = "Registration failed. Please try again." };
            }
        }


        public async Task<AuthDto> LoginAsync(TokenRequestModel model)
        {
            try
            {
                var authModel = new AuthDto();

                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user is null)
                {
                    await Task.Delay(500);
                    authModel.Message = "Invalid credentials";
                    return authModel;
                }
                if (!user.EmailConfirmed)
                {
                    return new AuthDto
                    {
                        Message = "Please verify your email address before logging in. Check your inbox.",
                        EmailVerificationRequired = true
                    };
                }
                // Check account lockout
                if (await _userManager.IsLockedOutAsync(user))
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                    var remainingTime = lockoutEnd?.Subtract(DateTimeOffset.UtcNow);

                    return new AuthDto
                    {
                        Message = $"Account is locked. Try again in {remainingTime?.Minutes ?? 0} minutes.",
                        IsAccountLocked = true,
                        LockoutEnd = lockoutEnd?.DateTime
                    };
                }

                if (!await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    await _userManager.AccessFailedAsync(user);
                    var failedAttempts = await _userManager.GetAccessFailedCountAsync(user);

                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        return new AuthDto
                        {
                            Message = "Account has been locked due to multiple failed login attempts.",
                            IsAccountLocked = true
                        };
                    }

                    var remainingAttempts = 5 - failedAttempts;
                    return new AuthDto
                    {
                        Message = $"Invalid email or password. {remainingAttempts} attempts remaining."
                    };
                }

                await _userManager.ResetAccessFailedCountAsync(user);

                var jwtSecurityToken = await CreateJwtToken(user);
                var rolesList = await _userManager.GetRolesAsync(user);
                var refreshToken = GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);


                authModel.IsAuthenticated = true;
                authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                authModel.Email = user.Email;
                authModel.Username = user.UserName;
                authModel.ExpiresOn = jwtSecurityToken.ValidTo;
                authModel.Roles = rolesList.ToList();

                return new AuthDto
                {
                    IsAuthenticated = true,
                    Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                    RefreshToken = refreshToken,
                    RefreshTokenExpiry = user.RefreshTokenExpiry,
                    Email = user.Email,
                    Username = user.UserName,
                    ExpiresOn = jwtSecurityToken.ValidTo,
                    Roles = rolesList.ToList(),
                    Credits = user.Credits,
                    Message = "Login successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for {Email}", model.Email);
                return new AuthDto { Message = "Login failed. Please try again." };
            }
        }
        public async Task<string> AddRoleAsync(AddRoleDto model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);

                if (user is null || !await _roleManager.RoleExistsAsync(model.Role))
                    return "Invalid user ID or Role";

                if (await _userManager.IsInRoleAsync(user, model.Role))
                    return "User already assigned to this role";

                var result = await _userManager.AddToRoleAsync(user, model.Role);

                return result.Succeeded ? string.Empty : "Something went wrong";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adding role failed for {Role}", model.Role);
                return "Something went wrong Adding role";
            }
        }
        public async Task<AuthDto> VerifyEmailAsync(string email, string token)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new AuthDto { Message = "Invalid verification request" };
            }

            if (user.EmailConfirmed)
            {
                return new AuthDto { Message = "Email is already verified" };
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                _logger.LogInformation("Email verified for user {UserId}", user.Id);
                return new AuthDto
                {
                    Message = "Email verified successfully! You can now log in.",
                    IsAuthenticated = true
                };
            }


            return new AuthDto { Message = "Email verification failed. The token may be invalid or expired." };
        }

        // Resend verification email
        //public async Task<AuthDto> ResendVerificationEmailAsync(string email)
        //{
        //    var user = await _userManager.FindByEmailAsync(email);
        //    if (user == null)
        //    {
        //        return new AuthDto { Message = "Email address not found" };
        //    }

        //    if (user.EmailConfirmed)
        //    {
        //        return new AuthDto { Message = "Email is already verified" };
        //    }

        //    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //    var emailSent = await _emailValidation.SendVerificationEmailAsync(
        //        user.Email, user.UserName, token);

        //    if (emailSent)
        //    {
        //        return new AuthDto { Message = "Verification email sent. Please check your inbox." };
        //    }

        //    return new AuthDto { Message = "Failed to send verification email. Please try again later." };
        //}

        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiresMinutes),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }
        public async Task<AuthDto> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken &&
                                         u.RefreshTokenExpiry > DateTime.UtcNow);

                if (user == null)
                    return new AuthDto { Message = "Invalid refresh token" };

                var newJwtToken = await CreateJwtToken(user);
                var newRefreshToken = GenerateRefreshToken();

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                user.LastLoginDate = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);

                return new AuthDto
                {
                    IsAuthenticated = true,
                    Token = new JwtSecurityTokenHandler().WriteToken(newJwtToken),
                    RefreshToken = newRefreshToken,
                    RefreshTokenExpiry = user.RefreshTokenExpiry,
                    Email = user.Email,
                    Username = user.UserName,
                    Credits = user.Credits,
                    ExpiresOn = newJwtToken.ValidTo,
                    Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                    Message = "Token refreshed successfully"

                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new AuthDto
                {
                    Message = ex.Message,
                };
            }
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}

