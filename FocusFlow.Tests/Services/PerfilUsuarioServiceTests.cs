using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using FocusFlowAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FocusFlow.Tests.Services
{
    public class PerfilUsuarioServiceTests : IDisposable
    {
        private readonly UsuarioContext _context;
        private readonly Mock<ILogger<PerfilUsuarioService>> _mockLogger;
        private readonly PerfilUsuarioService _service;
        private bool _disposed = false;

        public PerfilUsuarioServiceTests()
        {
            var options = new DbContextOptionsBuilder<UsuarioContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UsuarioContext(options);
            _mockLogger = new Mock<ILogger<PerfilUsuarioService>>();
            _service = new PerfilUsuarioService(_context, _mockLogger.Object);
        }

        [Fact]
        public async Task ObtenerPerfilAsync_DeberiaRetornarPerfil_CuandoExiste()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _context.PerfilUsuarios.Add(new PerfilUsuario 
            { 
                IdUsuario = userId, 
                Nombre = "Sofia", 
                Ocupacion = "Dev" 
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ObtenerPerfilAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Sofia", result!.Nombre);
        }

        [Fact]
        public async Task CrearPerfilAsync_DeberiaCrearNuevoPerfil_CuandoNoExiste()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new PerfilUsuarioDto { Nombre = "Nuevo", Ocupacion = "Estudiante" };

            // Act
            var result = await _service.CrearPerfilAsync(userId, dto);

            // Assert
            Assert.Equal("Nuevo", result.Nombre);
            var entity = await _context.PerfilUsuarios.FirstOrDefaultAsync(p => p.IdUsuario == userId);
            Assert.NotNull(entity);
            Assert.Equal(DateTimeKind.Utc, entity!.FechaRegistro.Kind);
        }

        [Fact]
        public async Task CrearPerfilAsync_DeberiaActualizarPerfil_CuandoYaExiste()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var original = new PerfilUsuario { IdUsuario = userId, Nombre = "Original", Rol = "admin" };
            _context.PerfilUsuarios.Add(original);
            await _context.SaveChangesAsync();

            var updateDto = new PerfilUsuarioDto { Nombre = "Modificado", Rol = "user" };

            // Act
            var result = await _service.CrearPerfilAsync(userId, updateDto);

            // Assert
            Assert.Equal("Modificado", result.Nombre);
            Assert.Equal("user", result.Rol);
            
            var entity = await _context.PerfilUsuarios.FirstAsync(p => p.IdUsuario == userId);
            Assert.Equal("Modificado", entity.Nombre);
            Assert.Equal("user", entity.Rol);
        }

        [Fact]
        public void Constructor_DeberiaLanzarExcepcion_CuandoArgumentosSonNulos()
        {
            Assert.Throws<ArgumentNullException>(() => new PerfilUsuarioService(null!, _mockLogger.Object));
            Assert.Throws<ArgumentNullException>(() => new PerfilUsuarioService(_context, null!));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Database.EnsureDeleted();
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }

        ~PerfilUsuarioServiceTests()
        {
            Dispose(false);
        }
    }
}