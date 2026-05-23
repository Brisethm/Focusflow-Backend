using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using FocusFlowAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FocusFlow.Tests.Services
{
    public class RecordatorioServiceTests : IDisposable
    {
        private readonly UsuarioContext _context;
        private readonly Mock<ILogger<RecordatorioService>> _mockLogger;
        private readonly RecordatorioService _service;

        public RecordatorioServiceTests()
        {
            var options = new DbContextOptionsBuilder<UsuarioContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new UsuarioContext(options);
            _mockLogger = new Mock<ILogger<RecordatorioService>>();
            _service = new RecordatorioService(_context, _mockLogger.Object);
        }

        [Fact]
        public async Task ObtenerRecordatoriosAsync_DeberiaRetornarSoloLosDelUsuario()
        {
            var userId = Guid.NewGuid();
            _context.Recordatorios.AddRange(
                new Recordatorio { IdUsuario = userId, Mensaje = "Mio", FechaHora = DateTime.UtcNow },
                new Recordatorio { IdUsuario = Guid.NewGuid(), Mensaje = "Otro", FechaHora = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            var result = await _service.ObtenerRecordatoriosAsync(userId);

            Assert.Single(result);
            Assert.Equal("Mio", result.First().Mensaje);
        }

        [Fact]
        public async Task CrearRecordatorioAsync_DeberiaLanzarExcepcion_CuandoFechaHoraEsNull()
        {
            var dto = new RecordatorioDto { Mensaje = "Sin fecha", FechaHora = null, Activo = true };
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CrearRecordatorioAsync(Guid.NewGuid(), dto));
        }

        [Fact]
        public async Task CrearRecordatorioAsync_DeberiaGuardarCorrectamente()
        {
            var userId = Guid.NewGuid();
            var fecha = DateTime.UtcNow;
            var dto = new RecordatorioDto { Mensaje = "Prueba", FechaHora = fecha, Tipo = "app", Activo = true };

            var result = await _service.CrearRecordatorioAsync(userId, dto);

            Assert.NotEqual(0, result.IdRecordatorio);
            var entity = await _context.Recordatorios.FindAsync(result.IdRecordatorio);
            Assert.NotNull(entity);
            Assert.Equal(userId, entity!.IdUsuario);
            Assert.Equal(DateTimeKind.Utc, entity.FechaHora.Kind);
        }

        [Fact]
        public async Task ObtenerRecordatorioPorIdAsync_DeberiaRetornarNull_CuandoNoPerteneceAlUsuario()
        {
            var userId = Guid.NewGuid();
            var recordatorio = new Recordatorio { IdUsuario = Guid.NewGuid(), Mensaje = "No es mio" };
            _context.Recordatorios.Add(recordatorio);
            await _context.SaveChangesAsync();

            var result = await _service.ObtenerRecordatorioPorIdAsync(userId, recordatorio.IdRecordatorio);

            Assert.Null(result);
        }

        [Fact]
        public async Task ActualizarRecordatorioAsync_DeberiaLanzarExcepcion_SiFechaEsNull()
        {
            var dto = new RecordatorioDto { Mensaje = "Sin mensaje", FechaHora = null, Activo = true };
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ActualizarRecordatorioAsync(Guid.NewGuid(), 1, dto));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Database.EnsureDeleted();
                _context.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
