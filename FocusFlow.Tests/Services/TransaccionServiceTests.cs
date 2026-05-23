using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using FocusFlowAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FocusFlow.Tests.Services
{
    public class TransaccionServiceTests : IDisposable
    {
        private readonly UsuarioContext _context;
        private readonly Mock<ILogger<TransaccionService>> _mockLogger;
        private readonly TransaccionService _service;
        private bool _disposed;

        public TransaccionServiceTests()
        {
            var options = new DbContextOptionsBuilder<UsuarioContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UsuarioContext(options);
            _mockLogger = new Mock<ILogger<TransaccionService>>();
            _service = new TransaccionService(_context, _mockLogger.Object);
        }

        [Fact]
        public async Task ObtenerTransaccionesAsync_DeberiaRetornarSoloTransaccionesDelUsuario()
        {
            // Arrange
            var idUsuario = Guid.NewGuid();
            var otherId = Guid.NewGuid();
            _context.Transacciones.AddRange(
                new Transaccion { IdUsuario = idUsuario, Monto = 100, Tipo = "Ingreso", Categoria = "Sueldo", Fecha = DateTime.UtcNow },
                new Transaccion { IdUsuario = otherId, Monto = 50, Tipo = "Gasto", Categoria = "Ocio", Fecha = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ObtenerTransaccionesAsync(idUsuario);

            // Assert
            Assert.Single(result);
            Assert.Equal(100, result.First().Monto);
        }

        [Fact]
        public async Task ObtenerTransaccionesAsync_DeberiaRetornarOrdenadosPorFechaDescendente()
        {
            // Arrange
            var idUsuario = Guid.NewGuid();
            var ahora = DateTime.UtcNow;
            _context.Transacciones.AddRange(
                new Transaccion { IdUsuario = idUsuario, Monto = 10, Fecha = ahora.AddDays(-1), Tipo = "A", Categoria = "C" },
                new Transaccion { IdUsuario = idUsuario, Monto = 20, Fecha = ahora, Tipo = "B", Categoria = "D" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = (await _service.ObtenerTransaccionesAsync(idUsuario)).ToList();

            // Assert
            Assert.Equal(20, result[0].Monto);
            Assert.Equal(10, result[1].Monto);
        }

        [Fact]
        public async Task CrearTransaccionAsync_DeberiaPersistirCorrectamente()
        {
            // Arrange
            var idUsuario = Guid.NewGuid();
            var dto = new TransaccionDto { Monto = 75, Tipo = "Gasto", Categoria = "Servicios", Descripcion = "Luz" };

            // Act
            var result = await _service.CrearTransaccionAsync(idUsuario, dto);

            // Assert
            Assert.NotEqual(0, result.IdTransaccion);
            var entity = await _context.Transacciones.FindAsync(result.IdTransaccion);
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