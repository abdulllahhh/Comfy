using Microsoft.AspNetCore.Mvc;
using Model.Dtos.Register;
using Model.Dtos.Request;
using Model.Interface;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Auth2Controller : ControllerBase
    {
        private readonly IAuthService _authService;

        public Auth2Controller(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(model);

            //if (!result.IsAuthenticated)
            //    return BadRequest(result.Message);

            return Ok(result);
        }

        //[HttpPost("token")]
        //public async Task<IActionResult> GetTokenAsync([FromBody] TokenRequestModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var result = await _authService.GetTokenAsync(model);

        //    //if (!result.IsAuthenticated)
        //    //    return BadRequest(result.Message);

        //    return Ok(result);
        //}
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RefreshTokenAsync(model.RefreshToken);

            if (string.IsNullOrEmpty(result.Token))
                return BadRequest(result);

            return Ok(result);
        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] TokenRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(model);

            //if (!result.IsAuthenticated)
            //    return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPost("addrole")]
        public async Task<IActionResult> AddRoleAsync([FromBody] AddRoleDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.AddRoleAsync(model);

            if (!string.IsNullOrEmpty(result))
                return BadRequest(result);

            return Ok(model);
        }


        //[HttpPost("verify-email")] //if we will send mails
        //public async Task<IActionResult> VerifyEmailAsync([FromBody] VerifyEmailDto model)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var result = await _authService.VerifyEmailAsync(model.Email, model.Token);
        //    return Ok(result);
        //}

        //[HttpPost("resend-verification")] //if we will send mails
        //public async Task<IActionResult> ResendVerificationEmailAsync([FromBody] ResendVerificationDto model)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var result = await _authService.ResendVerificationEmailAsync(model.Email);
        //    return Ok(result);
        //}
    }
}
