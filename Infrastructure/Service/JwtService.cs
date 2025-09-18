//using infrastructure.Data;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
//using Model.Dtos.Login;
//using Model.Entities;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;


//namespace Infrastructure.Service
//{
//    public class JwtService
//    {
//        private readonly IConfiguration _config;
//        private readonly UserManager<ApplicationUser> _userManager;

//        public JwtService(UserManager<ApplicationUser> um, IConfiguration configuration)
//        {
//            _config = configuration;
//            _userManager = um;
//        }

//        public async Task<LoginResponseDto?> Authenticate(LoginRequestDto request)
//        {
//            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
//            {
//                return null;
//            }
//            var user = await _userManager.FindByNameAsync(request.UserName);
//            if (user == null) return null;

//            var issuer = _config["Jwt:Issuer"];
//            var audience = _config["Jwt:Audience"];
//            var key = _config["Jwt:Key"];
//            var tokenValidityMins = _config.GetValue<int>("Jwt:ExpiresMinutes");
//            var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMins);
//            var tokenDescriptor = new SecurityTokenDescriptor
//            {
//                Subject = new ClaimsIdentity(new[]
//                {
//                    new Claim(JwtRegisteredClaimNames.Name,request.UserName)
//                }),
//                Expires = tokenExpiryTimeStamp,
//                Issuer = issuer,
//                Audience =  audience,
//                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
//                SecurityAlgorithms.HmacSha256Signature)
//            };
//            var tokenHandler = new JwtSecurityTokenHandler();
//            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
//            var accessToken = tokenHandler.WriteToken(securityToken);


//            return new LoginResponseDto
//            {
//                AccessToken = accessToken,
//                UserName = request.UserName,
//                ExpiredIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.UtcNow).TotalSeconds
//            };


//        }
//    }
//}
