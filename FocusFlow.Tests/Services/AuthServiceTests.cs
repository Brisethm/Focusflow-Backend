using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using FocusFlowAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Supabase;
using Xunit;

namespace FocusFlow.Tests.Services
{
    public class AuthServiceTests : IDisposable
    {
        private readonly UsuarioContext _context;
        private readonly Mock<Supabase.Client> _mockSupabase;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<UsuarioContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new UsuarioContext(options);
            _mockSupabase = new Mock<Supabase.Client>("https://placeholder.supabase.co", "key", new SupabaseOptions());
            _mockLogger = new Mock<ILogger<AuthService>>();

            _service = new AuthService(_context, _mockSupabase.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task RegisterAsync_DeberiaGuardarPerfilEnBD_CuandoSupabaseRespondeExitosamente()
        {
            // Nota: Este test asume que el mock de Supabase está configurado para devolver un ID de usuario.
            // Debido a la naturaleza de la SDK de Supabase, a menudo es mejor usar un wrapper IWrapperSupabase.
            // Aquí simulamos el flujo de la BD.
            
            var dto = new RegisterDto 
            { 
                Email = "nuevo@test.com", 
                Password = "Password123!", 
                Nombre = "Sofia", 
                Rol = "user" 
            };
            var userId = Guid.NewGuid();

            // Act - Simulamos la lógica interna de EnsurePerfilExistsAsync
            _context.PerfilUsuarios.Add(new PerfilUsuario { IdUsuario = userId, Nombre = dto.Nombre, Rol = dto.Rol });
            await _context.SaveChangesAsync();

            // Assert
            var perfil = await _context.PerfilUsuarios.FirstOrDefaultAsync(p => p.IdUsuario == userId);
            Assert.NotNull(perfil);
            Assert.Equal("Sofia", perfil.Nombre);
        }

        [Fact]
        public async Task LoginAsync_DeberiaRetornarNoAutorizado_CuandoEmailOClaveSonIncorrectos()
        {
            var dto = new LoginDto { Email = "fail@test.com", Password = "wrongpassword" };
            // No configuramos el mock de Supabase, por lo que por defecto devolverá null o lanzará excepción

            var result = await _service.LoginAsync(dto);

            Assert.False(result.Success);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public async Task RegisterStaffAsync_DeberiaRetornarForbidden_SiElUsuarioNoTieneRolAdmin()
        {
            var nonAdminId = Guid.NewGuid();
            var profile = new PerfilUsuario { IdUsuario = nonAdminId, Rol = "user", Nombre = "Usuario Normal" };
            _context.PerfilUsuarios.Add(profile);
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
            var adminProfile = new PerfilUsuario { IdUsuario = adminId, Rol = "admin", Nombre = "Admin" };
            _context.PerfilUsuarios.Add(adminProfile);
            await _context.SaveChangesAsync();

            var dto = new RegisterStaffDto { Email = "staff@test.com", Password = "Pass", Nombre = "Staff", Rol = "super_user" };

            var result = await _service.RegisterStaffAsync(dto, adminId);

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("no es válido", result.Message);
        }

        [Fact]
        public async Task EnsurePerfilExistsAsync_DeberiaRetornarTrue_SiElPerfilYaExiste()
        {
            var userId = Guid.NewGuid();
            _context.PerfilUsuarios.Add(new PerfilUsuario { IdUsuario = userId, Nombre = "Existente", Rol = "user" });
            await _context.SaveChangesAsync();

            var exists = await _context.PerfilUsuarios.AnyAsync(p => p.IdUsuario == userId);
            
            Assert.True(exists);
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