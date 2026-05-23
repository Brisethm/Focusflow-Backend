using FocusFlowAPI.DTOs;
using FocusFlowAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FocusFlowAPI.Extensions;

namespace FocusFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest(new { message = "Email, password y nombre son obligatorios." });

            var result = await _authService.RegisterAsync(dto);

            if (!result.Success)
                return StatusCode(result.StatusCode, new { message = result.Message });

            return Ok(new
            {
                message = result.Message,
                userId = result.UserId,
                token = result.Token,
                refreshToken = result.RefreshToken,
                profileReady = result.ProfileReady
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (!result.Success)
                return StatusCode(result.StatusCode, new { message = result.Message });

            return Ok(new
            {
                token = result.Token,
                refreshToken = result.RefreshToken,
                userId = result.UserId
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var success = await _authService.ResetPasswordAsync(dto.Email);
            if (!success) return BadRequest(new { message = "No se pudo enviar el correo de restablecimiento." });
            return Ok(new { message = "Correo de restablecimiento enviado" });
        }

        [HttpPost("update-password")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword([FromHeader(Name = "Authorization")] string authorization, [FromBody] UpdatePasswordDto dto)
        {
            var accessToken = authorization?.Replace("Bearer ", "").Trim();

            if (string.IsNullOrEmpty(accessToken))
                return Unauthorized(new { message = "Token no encontrado en el header." });

            var result = await _authService.UpdatePasswordAsync(accessToken, dto.NewPassword);

            if (!result.Success)
                return StatusCode(result.StatusCode, new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        [HttpPost("register-staff")]
        [Authorize]
        public async Task<IActionResult> RegisterStaff([FromBody] RegisterStaffDto dto)
        {
            var adminId = User.GetAuthenticatedUserId();
            if (adminId == null) return Unauthorized(new { message = "El token no contiene un identificador válido." });

            var result = await _authService.RegisterStaffAsync(dto, adminId.Value);

            if (!result.Success)
                return StatusCode(result.StatusCode, new { message = result.Message });

            return Ok(new { message = result.Message, userId = result.UserId });
        }
    }
}