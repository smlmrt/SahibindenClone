using Microsoft.AspNetCore.Mvc;
using SahibindenClone.Application.DTOs;
using SahibindenClone.Application.Services;

namespace SahibindenClone.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return result.Succeeded ? Ok(new { Message = "Kayıt başarıyla tamamlandı." })
                : BadRequest(new { Message = result.Message });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (!result.Succeeded) return Unauthorized(new { Message = result.Message });
            return Ok(new { result.Value!.Token, result.Value.UserId, result.Value.UserName, result.Value.Role });
        }

    }
}
