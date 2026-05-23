using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using FocusFlowAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FocusFlow.Tests.Services
{
    public class RegistroEmocionalServiceTests : IDisposable
    {
        private readonly UsuarioContext _context;
        private readonly Mock<ILogger<RegistroEmocionalService>> _mockLogger;
        private readonly RegistroEmocionalService _service;
        private bool _disposed;

        public RegistroEmocionalServiceTests()
        {
            var options = new DbContextOptionsBuilder<UsuarioContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UsuarioContext(options);
            _mockLogger = new Mock<ILogger<RegistroEmocionalService>>();
            _service = new RegistroEmocionalService(_context, _mockLogger.Object);
        }

        [Fact]
        public async Task ObtenerRegistrosAsync_DeberiaRetornarSoloRegistrosDelUsuario()
        {
            // Arrange
            var idUsuario = Guid.NewGuid();
            var otherId = Guid.NewGuid();
            _context.RegistrosEmocionales.AddRange(
                new RegistroEmocional { IdUsuario = idUsuario, EstadoAnimo = "Bien", Fecha = DateTime.UtcNow },
                new RegistroEmocional { IdUsuario = otherId, EstadoAnimo = "Mal", Fecha = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ObtenerRegistrosAsync(idUsuario);

            // Assert
            Assert.Single(result);
            Assert.Equal("Bien", result.First().EstadoAnimo);
        }

        [Fact]
        public async Task ObtenerRegistrosAsync_DeberiaRetornarOrdenadosPorFechaDescendente()
        {
            // Arrange
            var idUsuario = Guid.NewGuid();
            var ahora = DateTime.UtcNow;
            _context.RegistrosEmocionales.AddRange(
                new RegistroEmocional { IdUsuario = idUsuario, EstadoAnimo = "Antiguo", Fecha = ahora.AddDays(-1) },
                new RegistroEmocional { IdUsuario = idUsuario, EstadoAnimo = "Nuevo", Fecha = ahora }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = (await _service.ObtenerRegistrosAsync(idUsuario)).ToList();

            // Assert
            Assert.Equal("Nuevo", result[0].EstadoAnimo);
            Assert.Equal("Antiguo", result[1].EstadoAnimo);
        }

        [Fact]
        public async Task CrearRegistroAsync_DeberiaPersistirCorrectamente()
        {
            // Arrange
            var idUsuario = Guid.NewGuid();
            var dto = new RegistroEmocionalDto { EstadoAnimo = "Estresado", NivelEnergia = 3, NotaOpcional = "Mucho trabajo" };

            // Act
            var result = await _service.CrearRegistroAsync(idUsuario, dto);

            // Assert
            Assert.NotEqual(0, result.IdRegistro);
            var entity = await _context.RegistrosEmocionales.FindAsync(result.IdRegistro);
            Assert.NotNull(entity);
            Assert.Equal(idUsuario, entity!.IdUsuario);
            Assert.Equal(DateTimeKind.Utc, entity.Fecha.Kind);
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
                    _context.Database.EnsureDeleted();
                    _context.Dispose();
                }
                _disposed = true;
            }
        }
    }
}