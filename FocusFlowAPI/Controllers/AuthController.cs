using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Supabase;
using Supabase.Gotrue.Exceptions;

namespace FocusFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UsuarioContext _context;
        private readonly Supabase.Client _supabase;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UsuarioContext context, IConfiguration config, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;

            var supabaseUrl = config["Supabase:Url"];
            var supabaseKey = config["Supabase:AnonKey"];

            if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
                throw new InvalidOperationException("Las variables SUPABASE_URL o SUPABASE_ANON_KEY no estan configuradas.");

            _supabase = new Supabase.Client(
                supabaseUrl,
                supabaseKey,
                new SupabaseOptions { AutoRefreshToken = true }
            );
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return ValidationProblem(ModelState);

                if (string.IsNullOrWhiteSpace(dto.Email) ||
                    string.IsNullOrWhiteSpace(dto.Password) ||
                    string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    return BadRequest(new
                    {
                        message = "Email, password y nombre son obligatorios."
                    });
                }

                var response = await _supabase.Auth.SignUp(dto.Email, dto.Password);
                var session = ResolveSession(response);

                if (response?.User?.Id == null)
                    return BadRequest("No se pudo registrar el usuario en Supabase Auth.");

                if (!Guid.TryParse(response.User.Id, out var userId))
                    return BadRequest("Supabase devolvio un identificador de usuario invalido.");

                // Opcional: crear perfil local
                // var perfilCreado = await EnsurePerfilExistsAsync(userId, dto.Nombre);

                return Ok(new
                {
                    message = "Usuario registrado correctamente",
                    userId = response.User.Id,
                    token = session?.AccessToken ?? response.AccessToken,
                    refreshToken = session?.RefreshToken ?? response.RefreshToken,
                    // profileReady = perfilCreado
                });
            }
            catch (GotrueException ex) when (ex.Message.Contains("user_already_exists", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new
                {
                    message = "El correo ya esta registrado. Intenta iniciar sesion o recuperar tu contrasena."
                });
            }
            catch (GotrueException ex)
            {
                _logger.LogError(ex, "Error de Supabase durante el registro para {Email}", dto.Email);
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    message = "No se pudo completar el registro en el proveedor de autenticacion.",
                    detail = ex.Message
                });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error de base de datos al finalizar el registro para {Email}", dto.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "El usuario fue creado, pero ocurrio un error al guardar el perfil local.",
                    detail = ex.GetBaseException().Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado durante el registro para {Email}", dto.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Ocurrio un error inesperado durante el registro.",
                    detail = ex.Message
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var response = await _supabase.Auth.SignIn(dto.Email, dto.Password);
                var session = ResolveSession(response);

                if (response?.User?.Id == null)
                    return Unauthorized(new { message = "Credenciales invalidas." });

                return Ok(new
                {
                    token = session?.AccessToken ?? response.AccessToken,
                    refreshToken = session?.RefreshToken ?? response.RefreshToken,
                    userId = response.User.Id
                });
            }
            catch (GotrueException)
            {
                return Unauthorized(new { message = "Credenciales invalidas." });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var success = await _supabase.Auth.ResetPasswordForEmail(dto.Email);

            if (!success)
                return BadRequest("No se pudo enviar el correo de restablecimiento.");

            return Ok(new { message = "Correo de restablecimiento enviado" });
        }

        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
        {
            var user = await _supabase.Auth.Update(new Supabase.Gotrue.UserAttributes
            {
                Password = dto.NewPassword
            });

            if (user == null)
                return BadRequest("No se pudo actualizar la contrasena.");

            return Ok(new { message = "Contrasena actualizada correctamente" });
        }

        private async Task<bool> EnsurePerfilExistsAsync(Guid userId, string nombre)
        {
            var perfilExistente = await _context.PerfilUsuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdUsuario == userId);

            if (perfilExistente != null)
                return true;

            var perfil = new PerfilUsuario
            {
                IdUsuario = userId,
                Nombre = nombre
            };

            _context.PerfilUsuarios.Add(perfil);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException postgresEx &&
                                               postgresEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                _logger.LogWarning(ex, "El perfil para el usuario {UserId} ya existia al guardar.", userId);
                _context.Entry(perfil).State = EntityState.Detached;
                return true;
            }
        }

        private Supabase.Gotrue.Session? ResolveSession(Supabase.Gotrue.Session? session)
        {
            return session ?? _supabase.Auth.CurrentSession;
        }

        public class ResetPasswordDto
        {
            public required string Email { get; set; }
        }

        public class UpdatePasswordDto
        {
            public required string NewPassword { get; set; }
        }
    }
}
