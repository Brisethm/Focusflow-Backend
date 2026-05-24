using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using FocusFlowAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FocusFlow.Tests.Services
{
    public class PlanPersonalizadoServiceTests : IDisposable
    {
        private readonly UsuarioContext _context;
        private readonly Mock<ILogger<PlanPersonalizadoService>> _mockLogger;
        private readonly PlanPersonalizadoService _service;
        private bool _disposed = false;

        public PlanPersonalizadoServiceTests()
        {
            var options = new DbContextOptionsBuilder<UsuarioContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UsuarioContext(options);
            _mockLogger = new Mock<ILogger<PlanPersonalizadoService>>();
            _service = new PlanPersonalizadoService(_context, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_DeberiaLanzarArgumentNullException_CuandoParametrosSonNulos()
        {
            Assert.Throws<ArgumentNullException>(() => new PlanPersonalizadoService(null!, _mockLogger.Object));
            Assert.Throws<ArgumentNullException>(() => new PlanPersonalizadoService(_context, null!));
        }

        [Fact]
        public async Task ObtenerPlanesAsync_DeberiaRetornarSoloPlanesDelUsuario()
        {
            // Arrange
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            _context.PlanesPersonalizados.AddRange(
                new PlanPersonalizado { IdUsuario = userId1, EnfoqueDiario = 1 },
                new PlanPersonalizado { IdUsuario = userId2, EnfoqueDiario = 2 }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ObtenerPlanesAsync(userId1);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result.First().EnfoqueDiario);
        }

        [Fact]
        public async Task ObtenerPlanPorIdAsync_DeberiaRetornarPlan_CuandoPerteneceAlUsuario()
        {
            var userId = Guid.NewGuid();
            var plan = new PlanPersonalizado
            {
                IdUsuario = userId,
                EnfoqueDiario = 2,
                PausasDiarias = 1,
                HoraDescanso = new TimeOnly(21, 30)
            };
            _context.PlanesPersonalizados.Add(plan);
            await _context.SaveChangesAsync();

            var result = await _service.ObtenerPlanPorIdAsync(userId, plan.IdPlan);

            Assert.NotNull(result);
            Assert.Equal(plan.IdPlan, result!.IdPlan);
            Assert.Equal(userId, result.IdUsuario);
        }

        [Fact]
        public async Task ObtenerPlanPorIdAsync_DeberiaRetornarNull_CuandoNoPerteneceAlUsuario()
        {
            var ownerId = Guid.NewGuid();
            var otherId = Guid.NewGuid();
            var plan = new PlanPersonalizado { IdUsuario = ownerId, EnfoqueDiario = 2 };
            _context.PlanesPersonalizados.Add(plan);
            await _context.SaveChangesAsync();

            var result = await _service.ObtenerPlanPorIdAsync(otherId, plan.IdPlan);

            Assert.Null(result);
        }

        [Fact]
        public async Task CrearPlanAsync_DeberiaPersistirPlanConFechaUtc()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new CreatePlanDto 
            { 
                HoraDescanso = new TimeOnly(22, 0),
                EnfoqueDiario = 8,
                PausasDiarias = 3
            };

            // Act
            var result = await _service.CrearPlanAsync(userId, dto);

            // Assert
            Assert.NotEqual(0, result.IdPlan);
            var entity = await _context.PlanesPersonalizados.FindAsync(result.IdPlan);
            Assert.NotNull(entity);
            Assert.Equal(DateTimeKind.Utc, entity!.FechaCreacion.Kind);
            Assert.Equal(userId, entity.IdUsuario);
        }

        [Fact]
        public async Task CrearPlanAsync_DeberiaUsarValoresPorDefecto_CuandoSonNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new CreatePlanDto { HoraDescanso = null, EnfoqueDiario = null, PausasDiarias = null };

            // Act
            var result = await _service.CrearPlanAsync(userId, dto);

            // Assert
            Assert.Equal(TimeOnly.MinValue, result.HoraDescanso);
            Assert.Equal(0, result.EnfoqueDiario);
            Assert.Equal(0, result.PausasDiarias);
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

        ~PlanPersonalizadoServiceTests()
        {
            Dispose(false);
        }
    }
}
