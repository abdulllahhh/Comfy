using Model.Dtos.Register;

namespace Model.Interface
{
    public interface IAuthService
    {
        Task<AuthDto> RegisterAsync(RegisterDto model);
        //Task<AuthDto> GetTokenAsync(TokenRequestModel model);
        Task<string> AddRoleAsync(AddRoleDto model);
        //Task<AuthDto> VerifyEmailAsync(string email, string token);
        Task<AuthDto> LoginAsync(TokenRequestModel model);
        Task<AuthDto> RefreshTokenAsync(string refreshToken);
        //Task<AuthDto> ResendVerificationEmailAsync(string email);
    }
}
