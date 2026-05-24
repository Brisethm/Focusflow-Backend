using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Supabase.Gotrue.Exceptions;

namespace FocusFlowAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UsuarioContext _context;
        private readonly ISupabaseAuthClient _supabaseAuth;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UsuarioContext context, ISupabaseAuthClient supabaseAuth, ILogger<AuthService> logger)
        {
            _context = context;
            _supabaseAuth = supabaseAuth;
            _logger = logger;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterDto dto)
        {
            try
            {
                var signUpOptions = new Supabase.Gotrue.SignUpOptions
                {
                    Data = new Dictionary<string, object>
                    {
                        { "nombre", dto.Nombre },
                        { "rol", dto.Rol }
                    }
                };

                var response = await _supabaseAuth.SignUpAsync(dto.Email, dto.Password, signUpOptions);
                var session = ResolveSession(response);

                if (response?.User?.Id == null || !Guid.TryParse(response.User.Id, out var userId))
                {
                    return new AuthResponse { Success = false, StatusCode = 400, Message = "No se pudo registrar el usuario en Supabase Auth." };
                }

                var perfilCreado = await EnsurePerfilExistsAsync(userId, dto.Nombre, dto.Rol);

                return new AuthResponse
                {
                    Success = true,
                    Message = "Usuario registrado correctamente",
                    UserId = response.User.Id,
                    Token = session?.AccessToken ?? response.AccessToken,
                    RefreshToken = session?.RefreshToken ?? response.RefreshToken,
                    ProfileReady = perfilCreado
                };
            }
            catch (GotrueException ex) when (ex.Message.Contains("user_already_exists", StringComparison.OrdinalIgnoreCase))
            {
                return new AuthResponse { Success = false, StatusCode = 409, Message = "El correo ya está registrado. Intenta iniciar sesión o recuperar tu contraseña." };
            }
            catch (GotrueException ex)
            {
                _logger.LogError(ex, "Error de Supabase durante el registro para {Email}", dto.Email);
                return new AuthResponse { Success = false, StatusCode = 502, Message = "No se pudo completar el registro en el proveedor de autenticación." };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error de BD al finalizar el registro para {Email}", dto.Email);
                return new AuthResponse { Success = false, StatusCode = 500, Message = "El usuario fue creado, pero ocurrió un error al guardar el perfil local." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado durante el registro para {Email}", dto.Email);
                return new AuthResponse { Success = false, StatusCode = 500, Message = "Ocurrió un error inesperado durante el registro." };
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginDto dto)
        {
            try
            {
                var response = await _supabaseAuth.SignInAsync(dto.Email, dto.Password);
                var session = ResolveSession(response);

                if (response?.User?.Id == null)
                    return new AuthResponse { Success = false, StatusCode = 401, Message = "Credenciales inválidas." };

                return new AuthResponse
                {
                    Success = true,
                    Token = session?.AccessToken ?? response.AccessToken,
                    RefreshToken = session?.RefreshToken ?? response.RefreshToken,
                    UserId = response.User.Id
                };
            }
            catch (GotrueException)
            {
                return new AuthResponse { Success = false, StatusCode = 401, Message = "Credenciales inválidas." };
            }
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            return await _supabaseAuth.ResetPasswordForEmailAsync(email);
        }

        public async Task<AuthResponse> UpdatePasswordAsync(string accessToken, string newPassword)
        {
            try
            {
                await _supabaseAuth.SetSessionAsync(accessToken, accessToken);
                var user = await _supabaseAuth.UpdateAsync(new Supabase.Gotrue.UserAttributes
                {
                    Password = newPassword
                });

                if (user == null)
                    return new AuthResponse { Success = false, StatusCode = 400, Message = "No se pudo actualizar la contraseña." };

                return new AuthResponse { Success = true, Message = "Contraseña actualizada correctamente" };
            }
            catch (GotrueException ex)
            {
                _logger.LogError(ex, "Error de GoTrue al actualizar password");
                return new AuthResponse { Success = false, StatusCode = 400, Message = ex.Message };
            }
        }

        public async Task<AuthResponse> RegisterStaffAsync(RegisterStaffDto dto, Guid adminId)
        {
            try
            {
                var adminProfile = await _context.PerfilUsuarios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.IdUsuario == adminId);

                if (adminProfile == null || adminProfile.Rol != "admin")
                    return new AuthResponse { Success = false, StatusCode = 403, Message = "Acceso denegado. Solo administradores." };

                var rolesPermitidos = new[] { "admin", "support" };
                if (!rolesPermitidos.Contains(dto.Rol.ToLower()))
                    return new AuthResponse { Success = false, StatusCode = 400, Message = "El rol especificado no es válido." };

                var signUpOptions = new Supabase.Gotrue.SignUpOptions
                {
                    Data = new Dictionary<string, object> { { "nombre", dto.Nombre }, { "rol", dto.Rol.ToLower() } }
                };

                var response = await _supabaseAuth.SignUpAsync(dto.Email, dto.Password, signUpOptions);

                if (response?.User?.Id == null || !Guid.TryParse(response.User.Id, out var newUserId))
                    return new AuthResponse { Success = false, StatusCode = 400, Message = "No se pudo registrar la cuenta en Supabase." };

                await EnsurePerfilExistsAsync(newUserId, dto.Nombre, dto.Rol.ToLower());

                return new AuthResponse { Success = true, Message = $"Cuenta de {dto.Rol} creada exitosamente para {dto.Nombre}", UserId = response.User.Id };
            }
            catch (GotrueException ex) when (ex.Message.Contains("user_already_exists", StringComparison.OrdinalIgnoreCase))
            {
                return new AuthResponse { Success = false, StatusCode = 409, Message = "El correo ya está registrado en el sistema." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cuenta de staff.");
                return new AuthResponse { Success = false, StatusCode = 500, Message = "Ocurrió un error inesperado." };
            }
        }

        private async Task<bool> EnsurePerfilExistsAsync(Guid userId, string nombre, string rol)
        {
            var perfilExistente = await _context.PerfilUsuarios.AsNoTracking().FirstOrDefaultAsync(p => p.IdUsuario == userId);
            if (perfilExistente != null) return true;

            var perfil = new PerfilUsuario { IdUsuario = userId, Nombre = nombre, Rol = rol };
            _context.PerfilUsuarios.Add(perfil);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                _logger.LogWarning(ex, "El perfil para el usuario {UserId} ya existía al guardar.", userId);
                _context.Entry(perfil).State = EntityState.Detached;
                return true;
            }
        }

        private Supabase.Gotrue.Session? ResolveSession(Supabase.Gotrue.Session? session)
        {
            return session ?? _supabaseAuth.CurrentSession;
        }
    }
}
