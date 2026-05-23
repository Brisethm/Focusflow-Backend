using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using FocusFlowAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FocusFlow.Tests.Services
{
    public class SesionEnfoqueServiceTests : IDisposable
    {
        private readonly UsuarioContext _context;
        private readonly Mock<ILogger<SesionEnfoqueService>> _mockLogger;
        private readonly SesionEnfoqueService _service;

        public SesionEnfoqueServiceTests()
        {
            var options = new DbContextOptionsBuilder<UsuarioContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new UsuarioContext(options);
            _mockLogger = new Mock<ILogger<SesionEnfoqueService>>();
            _service = new SesionEnfoqueService(_context, _mockLogger.Object);
        }

        [Fact]
        public async Task ObtenerSesionesAsync_DeberiaRetornarSoloLasDelUsuario()
        {
            var userId = Guid.NewGuid();
            _context.SesionesEnfoque.AddRange(
                new SesionEnfoque { IdUsuario = userId, Tipo = "work", DuracionMinutos = 25, Fecha = DateTime.UtcNow },
                new SesionEnfoque { IdUsuario = Guid.NewGuid(), Tipo = "break", DuracionMinutos = 5, Fecha = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            var result = await _service.ObtenerSesionesAsync(userId);

            Assert.Single(result);
            Assert.Equal("work", result.First().Tipo);
        }

        [Fact]
        public async Task CrearSesionAsync_DeberiaPersistirCorrectamente()
        {
            var userId = Guid.NewGuid();
            var dto = new SesionEnfoqueDto { DuracionMinutos = 50, Tipo = "deep work" };

            var result = await _service.CrearSesionAsync(userId, dto);

            Assert.NotEqual(0, result.IdSesion);
            var entity = await _context.SesionesEnfoque.FindAsync(result.IdSesion);
            Assert.NotNull(entity);
            Assert.Equal(userId, entity!.IdUsuario);
            Assert.Equal(DateTimeKind.Utc, entity.Fecha.Kind);
        }

        [Fact]
        public async Task ObtenerSesionPorIdAsync_DeberiaRetornarNull_CuandoNoPerteneceAlUsuario()
        {
            var userId = Guid.NewGuid();
            var sesion = new SesionEnfoque { IdUsuario = Guid.NewGuid(), Tipo = "otro", DuracionMinutos = 10 };
            _context.SesionesEnfoque.Add(sesion);
            await _context.SaveChangesAsync();

            var result = await _service.ObtenerSesionPorIdAsync(userId, sesion.IdSesion);

            Assert.Null(result);
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