using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using FocusFlowAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Supabase.Gotrue;
using System.Reflection;
using Xunit;

namespace FocusFlow.Tests.Services
{
    public class AuthServiceTests : IDisposable
    {
        private readonly UsuarioContext _context;
        private readonly Mock<ISupabaseAuthClient> _mockSupabaseAuth;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<UsuarioContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UsuarioContext(options);
            _mockSupabaseAuth = new Mock<ISupabaseAuthClient>();
            _mockLogger = new Mock<ILogger<AuthService>>();

            _service = new AuthService(_context, _mockSupabaseAuth.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task RegisterAsync_DeberiaGuardarPerfilEnBD_CuandoSupabaseRespondeExitosamente()
        {
            var dto = new RegisterDto
            {
                Email = "nuevo@test.com",
                Password = "Password123!",
                Nombre = "Sofia",
                Rol = "user"
            };
            var userId = Guid.NewGuid();
            _mockSupabaseAuth
                .Setup(s => s.SignUpAsync(dto.Email, dto.Password, It.IsAny<SignUpOptions>()))
                .ReturnsAsync(CreateSession(userId.ToString(), "access-token", "refresh-token"));

            var result = await _service.RegisterAsync(dto);

            Assert.True(result.Success);
            Assert.Equal(userId.ToString(), result.UserId);
            Assert.Equal("access-token", result.Token);
            Assert.Equal("refresh-token", result.RefreshToken);
            Assert.True(result.ProfileReady);

            var perfil = await _context.PerfilUsuarios.SingleAsync(p => p.IdUsuario == userId);
            Assert.Equal("Sofia", perfil.Nombre);
            Assert.Equal("user", perfil.Rol);
        }

        [Fact]
        public async Task RegisterAsync_DeberiaRetornarBadRequest_CuandoProveedorNoRetornaUsuarioValido()
        {
            var dto = new RegisterDto
            {
                Email = "nuevo@test.com",
                Password = "Password123!",
                Nombre = "Sofia",
                Rol = "user"
            };
            _mockSupabaseAuth
                .Setup(s => s.SignUpAsync(dto.Email, dto.Password, It.IsAny<SignUpOptions>()))
                .ReturnsAsync(CreateSession("id-no-guid"));

            var result = await _service.RegisterAsync(dto);

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Supabase Auth", result.Message);
        }

        [Fact]
        public async Task RegisterAsync_DeberiaRetornarError500_CuandoProveedorLanzaExcepcion()
        {
            var dto = new RegisterDto
            {
                Email = "nuevo@test.com",
                Password = "Password123!",
                Nombre = "Sofia",
                Rol = "user"
            };
            _mockSupabaseAuth
                .Setup(s => s.SignUpAsync(dto.Email, dto.Password, It.IsAny<SignUpOptions>()))
                .ThrowsAsync(new InvalidOperationException("Proveedor caido"));

            var result = await _service.RegisterAsync(dto);

            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Contains("inesperado", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task LoginAsync_DeberiaRetornarNoAutorizado_CuandoEmailOClaveSonIncorrectos()
        {
            var dto = new LoginDto { Email = "fail@test.com", Password = "wrongpassword" };

            var result = await _service.LoginAsync(dto);

            Assert.False(result.Success);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public async Task LoginAsync_DeberiaRetornarOk_CuandoCredencialesSonCorrectas()
        {
            var dto = new LoginDto { Email = "ok@test.com", Password = "Password123!" };
            var userId = Guid.NewGuid();
            _mockSupabaseAuth
                .Setup(s => s.SignInAsync(dto.Email, dto.Password))
                .ReturnsAsync(CreateSession(userId.ToString(), "jwt", "refresh"));

            var result = await _service.LoginAsync(dto);

            Assert.True(result.Success);
            Assert.Equal(userId.ToString(), result.UserId);
            Assert.Equal("jwt", result.Token);
            Assert.Equal("refresh", result.RefreshToken);
        }

        [Fact]
        public async Task ResetPasswordAsync_DeberiaDelegarEnProveedor()
        {
            _mockSupabaseAuth
                .Setup(s => s.ResetPasswordForEmailAsync("test@test.com"))
                .ReturnsAsync(true);

            var result = await _service.ResetPasswordAsync("test@test.com");

            Assert.True(result);
        }

        [Fact]
        public async Task UpdatePasswordAsync_DeberiaRetornarBadRequest_CuandoProveedorNoRetornaUsuario()
        {
            _mockSupabaseAuth.Setup(s => s.SetSessionAsync("token", "token")).Returns(Task.CompletedTask);
            _mockSupabaseAuth.Setup(s => s.UpdateAsync(It.IsAny<UserAttributes>()))
                .ReturnsAsync((User?)null);

            var result = await _service.UpdatePasswordAsync("token", "NewPassword123!");

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("actualizar", result.Message);
        }

        [Fact]
        public async Task UpdatePasswordAsync_DeberiaRetornarOk_CuandoProveedorActualizaUsuario()
        {
            _mockSupabaseAuth.Setup(s => s.SetSessionAsync("token", "token")).Returns(Task.CompletedTask);
            _mockSupabaseAuth.Setup(s => s.UpdateAsync(It.IsAny<UserAttributes>()))
                .ReturnsAsync(new User());

            var result = await _service.UpdatePasswordAsync("token", "NewPassword123!");

            Assert.True(result.Success);
            Assert.Contains("actualizada", result.Message);
        }

        [Fact]
        public async Task RegisterStaffAsync_DeberiaRetornarForbidden_SiElAdminNoExiste()
        {
            var dto = new RegisterStaffDto
            {
                Email = "staff@test.com",
                Password = "Pass",
                Nombre = "Staff",
                Rol = "support"
            };

            var result = await _service.RegisterStaffAsync(dto, Guid.NewGuid());

            Assert.False(result.Success);
            Assert.Equal(403, result.StatusCode);
            Assert.Equal("Acceso denegado. Solo administradores.", result.Message);
        }

        [Fact]
        public async Task RegisterStaffAsync_DeberiaRetornarForbidden_SiElUsuarioNoTieneRolAdmin()
        {
            var nonAdminId = Guid.NewGuid();
            _context.PerfilUsuarios.Add(new PerfilUsuario { IdUsuario = nonAdminId, Rol = "user", Nombre = "Usuario Normal" });
            await _context.SaveChangesAsync();

            var dto = new RegisterStaffDto { Email = "staff@test.com", Password = "Pass", Nombre = "Staff", Rol = "support" };

            var result = await _service.RegisterStaffAsync(dto, nonAdminId);

            Assert.False(result.Success);
            Assert.Equal(403, result.StatusCode);
            Assert.Equal("Acceso denegado. Solo administradores.", result.Message);
        }

        [Fact]
        public async Task RegisterStaffAsync_DeberiaRetornarBadRequest_SiElRolEsInvalido()
        {
            var adminId = Guid.NewGuid();
            _context.PerfilUsuarios.Add(new PerfilUsuario { IdUsuario = adminId, Rol = "admin", Nombre = "Admin" });
            await _context.SaveChangesAsync();

            var dto = new RegisterStaffDto { Email = "staff@test.com", Password = "Pass", Nombre = "Staff", Rol = "super_user" };

            var result = await _service.RegisterStaffAsync(dto, adminId);

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("no es", result.Message);
        }

        [Fact]
        public async Task RegisterStaffAsync_DeberiaCrearCuenta_CuandoAdminYRolSonValidos()
        {
            var adminId = Guid.NewGuid();
            var staffId = Guid.NewGuid();
            _context.PerfilUsuarios.Add(new PerfilUsuario { IdUsuario = adminId, Rol = "admin", Nombre = "Admin" });
            await _context.SaveChangesAsync();

            var dto = new RegisterStaffDto
            {
                Email = "staff@test.com",
                Password = "Pass",
                Nombre = "Staff",
                Rol = "SUPPORT"
            };
            _mockSupabaseAuth
                .Setup(s => s.SignUpAsync(dto.Email, dto.Password, It.IsAny<SignUpOptions>()))
                .ReturnsAsync(CreateSession(staffId.ToString()));

            var result = await _service.RegisterStaffAsync(dto, adminId);

            Assert.True(result.Success);
            Assert.Equal(staffId.ToString(), result.UserId);
            var perfil = await _context.PerfilUsuarios.SingleAsync(p => p.IdUsuario == staffId);
            Assert.Equal("support", perfil.Rol);
            Assert.Equal("Staff", perfil.Nombre);
        }

        [Fact]
        public async Task RegisterStaffAsync_DeberiaRetornarBadRequest_CuandoProveedorNoRetornaUsuarioValido()
        {
            var adminId = Guid.NewGuid();
            _context.PerfilUsuarios.Add(new PerfilUsuario { IdUsuario = adminId, Rol = "admin", Nombre = "Admin" });
            await _context.SaveChangesAsync();

            var dto = new RegisterStaffDto
            {
                Email = "staff@test.com",
                Password = "Pass",
                Nombre = "Staff",
                Rol = "support"
            };
            _mockSupabaseAuth
                .Setup(s => s.SignUpAsync(dto.Email, dto.Password, It.IsAny<SignUpOptions>()))
                .ReturnsAsync(CreateSession("id-invalido"));

            var result = await _service.RegisterStaffAsync(dto, adminId);

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Supabase", result.Message);
        }

        [Fact]
        public async Task RegisterStaffAsync_DeberiaRetornarError500_CuandoProveedorLanzaExcepcion()
        {
            var adminId = Guid.NewGuid();
            _context.PerfilUsuarios.Add(new PerfilUsuario { IdUsuario = adminId, Rol = "admin", Nombre = "Admin" });
            await _context.SaveChangesAsync();

            var dto = new RegisterStaffDto
            {
                Email = "staff@test.com",
                Password = "Pass",
                Nombre = "Staff",
                Rol = "support"
            };
            _mockSupabaseAuth
                .Setup(s => s.SignUpAsync(dto.Email, dto.Password, It.IsAny<SignUpOptions>()))
                .ThrowsAsync(new InvalidOperationException("Proveedor caido"));

            var result = await _service.RegisterStaffAsync(dto, adminId);

            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public async Task EnsurePerfilExistsAsync_DeberiaRetornarTrue_SiElPerfilYaExiste()
        {
            var userId = Guid.NewGuid();
            _context.PerfilUsuarios.Add(new PerfilUsuario { IdUsuario = userId, Nombre = "Existente", Rol = "user" });
            await _context.SaveChangesAsync();

            var exists = await InvokeEnsurePerfilExistsAsync(userId, "Otro Nombre", "admin");

            Assert.True(exists);
            var perfiles = await _context.PerfilUsuarios.Where(p => p.IdUsuario == userId).ToListAsync();
            Assert.Single(perfiles);
            Assert.Equal("Existente", perfiles[0].Nombre);
            Assert.Equal("user", perfiles[0].Rol);
        }

        [Fact]
        public async Task EnsurePerfilExistsAsync_DeberiaCrearPerfil_CuandoNoExiste()
        {
            var userId = Guid.NewGuid();

            var result = await InvokeEnsurePerfilExistsAsync(userId, "Nuevo", "support");

            Assert.True(result);
            var perfil = await _context.PerfilUsuarios.SingleAsync(p => p.IdUsuario == userId);
            Assert.Equal("Nuevo", perfil.Nombre);
            Assert.Equal("support", perfil.Rol);
        }

        private Task<bool> InvokeEnsurePerfilExistsAsync(Guid userId, string nombre, string rol)
        {
            var method = typeof(AuthService).GetMethod("EnsurePerfilExistsAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);

            var task = method!.Invoke(_service, new object[] { userId, nombre, rol });
            return Assert.IsAssignableFrom<Task<bool>>(task);
        }

        private static Session CreateSession(string? userId, string? accessToken = null, string? refreshToken = null)
        {
            return new Session
            {
                User = userId is null ? null : new User { Id = userId },
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Database.EnsureDeleted();
                _context.Dispose();
            }
        }
    }
}
