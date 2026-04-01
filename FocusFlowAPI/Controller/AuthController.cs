using Microsoft.AspNetCore.Mvc;
using FocusFlowAPI.Models;
using FocusFlowAPI.DTOs;
using Supabase;
using System.Threading.Tasks;

namespace FocusFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UsuarioContext _context;
        private readonly Supabase.Client _supabase;

        public AuthController(UsuarioContext context, IConfiguration config)
        {
            _context = context;

            var supabaseUrl = config["SUPABASE_URL"];
            var supabaseKey = config["SUPABASE_KEY"];

            if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
                throw new InvalidOperationException("Las variables SUPABASE_URL o SUPABASE_KEY no están configuradas.");

            _supabase = new Supabase.Client(
                supabaseUrl,
                supabaseKey,
                new SupabaseOptions { AutoRefreshToken = true }
            );
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var response = await _supabase.Auth.SignUp(dto.Email, dto.Password);

            if (response?.User?.Id == null)
                return BadRequest("No se pudo registrar el usuario en Supabase Auth.");

            var perfil = new PerfilUsuario
            {
                IdUsuario = Guid.Parse(response.User.Id),
                Nombre = dto.Nombre
            };

            _context.PerfilUsuarios.Add(perfil);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario registrado correctamente", userId = response.User.Id });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var response = await _supabase.Auth.SignIn(dto.Email, dto.Password);

            if (response?.User?.Id == null)
                return Unauthorized("Credenciales inválidas.");

            return Ok(new { token = response.AccessToken, userId = response.User.Id });
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var success = await _supabase.Auth.ResetPasswordForEmail(dto.Email);

            if (!success)
                return BadRequest("No se pudo enviar el correo de restablecimiento.");

            return Ok(new { message = "Correo de restablecimiento enviado" });
        }

        public class ResetPasswordDto
        {
            public required string Email { get; set; }
        }

        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
        {
            var user = await _supabase.Auth.Update(new Supabase.Gotrue.UserAttributes
            {
                Password = dto.NewPassword
            });

            if (user == null)
                return BadRequest("No se pudo actualizar la contraseña.");

            return Ok(new { message = "Contraseña actualizada correctamente" });
        }

        public class UpdatePasswordDto
        {
            public required string NewPassword { get; set; }
        }


    }
}