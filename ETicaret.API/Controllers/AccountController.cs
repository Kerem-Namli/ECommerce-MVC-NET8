using ETicaret.Business.Abstract;
using ETicaret.Entity.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ETicaret.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userToLogin = await _authService.LoginAsync(userForLoginDto);
            if (!userToLogin.Success)
            {
                return BadRequest(userToLogin);
            }

            return Ok(userToLogin);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            var registerResult = await _authService.RegisterAsync(userForRegisterDto, userForRegisterDto.Password);
            if (registerResult.Success)
            {
                var tokenResult = _authService.CreateAccessToken(registerResult.Data);
                if (tokenResult.Success)
                {
                    return Ok(tokenResult);
                }
            }

            return BadRequest(registerResult);
        }
    }
}
