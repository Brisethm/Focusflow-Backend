using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using FocusFlowAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FocusFlow.Tests.Services
{
    public class TareaServiceTests : IDisposable
    {
        private readonly UsuarioContext _context;
        private readonly Mock<ILogger<TareaService>> _mockLogger;
        private readonly TareaService _service;

        public TareaServiceTests()
        {
            var options = new DbContextOptionsBuilder<UsuarioContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UsuarioContext(options);
            _mockLogger = new Mock<ILogger<TareaService>>();
            _service = new TareaService(_context, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_DeberiaLanzarArgumentNullException_CuandoParametrosSonNulos()
        {
            Assert.Throws<ArgumentNullException>(() => new TareaService(null!, _mockLogger.Object));
            Assert.Throws<ArgumentNullException>(() => new TareaService(_context, null!));
        }

        [Fact]
        public async Task ObtenerTareasAsync_DeberiaRetornarSoloTareasDelUsuario()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            _context.Tareas.AddRange(
                new Tarea { IdUsuario = userId, Titulo = "Tarea 1", Prioridad = "Alta", NivelEsfuerzo = "Medio", Estado = "Pendiente" },
                new Tarea { IdUsuario = userId, Titulo = "Tarea 2", Prioridad = "Baja", NivelEsfuerzo = "Bajo", Estado = "Completada" },
                new Tarea { IdUsuario = otherUserId, Titulo = "Tarea Ajena", Prioridad = "Alta", NivelEsfuerzo = "Medio", Estado = "Pendiente" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ObtenerTareasAsync(userId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, t => Assert.NotEqual("Tarea Ajena", t.Titulo));
        }

        [Fact]
        public async Task ObtenerTareaPorIdAsync_DeberiaRetornarTarea_CuandoPerteneceAlUsuario()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var tarea = new Tarea { IdUsuario = userId, Titulo = "Mi Tarea", Prioridad = "Alta", NivelEsfuerzo = "Medio", Estado = "Pendiente" };
            _context.Tareas.Add(tarea);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ObtenerTareaPorIdAsync(userId, tarea.IdTarea);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Mi Tarea", result!.Titulo);
        }

        [Fact]
        public async Task ObtenerTareaPorIdAsync_DeberiaRetornarNull_CuandoNoExisteOPerteneceAOtroUsuario()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var tarea = new Tarea { IdUsuario = otherUserId, Titulo = "Ajena", Prioridad = "Alta", NivelEsfuerzo = "Medio", Estado = "Pendiente" };
            _context.Tareas.Add(tarea);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ObtenerTareaPorIdAsync(userId, tarea.IdTarea);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CrearTareaAsync_DeberiaPersistirTareaYRetornarDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new TareaRequestDto
            {
                Titulo = "Nueva Tarea",
                Descripcion = "Desc",
                Prioridad = "Media",
                NivelEsfuerzo = "Bajo",
                Estado = "Pendiente",
                FechaLimite = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var result = await _service.CrearTareaAsync(userId, dto);

            // Assert
            Assert.NotEqual(0, result.IdTarea);
            Assert.Equal(dto.Titulo, result.Titulo);
            
            var entity = await _context.Tareas.FindAsync(result.IdTarea);
            Assert.NotNull(entity);
            Assert.Equal(userId, entity!.IdUsuario);
            Assert.Equal(DateTimeKind.Utc, entity.FechaCreacion.Kind);
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _context.Database.EnsureDeleted();
                _context.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}