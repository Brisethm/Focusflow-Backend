using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using FocusFlowAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FocusFlow.Tests.Services
{
    public class CuestionarioServiceTests : IDisposable
    {
        private readonly UsuarioContext _context;
        private readonly Mock<ILogger<CuestionarioService>> _mockLogger;
        private readonly CuestionarioService _service;

        public CuestionarioServiceTests()
        {
            var options = new DbContextOptionsBuilder<UsuarioContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UsuarioContext(options);
            _mockLogger = new Mock<ILogger<CuestionarioService>>();
            _service = new CuestionarioService(_context, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_DeberiaLanzarExcepcion_CuandoArgumentosSonNulos()
        {
            _ = Assert.Throws<ArgumentNullException>(() => new CuestionarioService(null!, _mockLogger.Object));
            _ = Assert.Throws<ArgumentNullException>(() => new CuestionarioService(_context, null!));
        }

        [Fact]
        public async Task ObtenerCuestionariosAsync_DeberiaRetornarSoloCuestionariosDelUsuario()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            _context.Cuestionarios.AddRange(
                new Cuestionario { IdUsuario = userId, PuntajeTotal = 10, Completado = true },
                new Cuestionario { IdUsuario = otherUserId, PuntajeTotal = 20, Completado = true }
            );
            _ = await _context.SaveChangesAsync();

            // Act
            var result = await _service.ObtenerCuestionariosAsync(userId);

            // Assert
            _ = Assert.Single(result);
            Assert.Equal(10, result.First().PuntajeTotal);
        }

        [Fact]
        public async Task ObtenerCuestionariosAsync_DeberiaRetornarOrdenadosPorFechaDescendente()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var hoy = DateTime.UtcNow;
            
            _context.Cuestionarios.AddRange(
                new Cuestionario { IdUsuario = userId, Fecha = hoy.AddDays(-1), PuntajeTotal = 50 },
                new Cuestionario { IdUsuario = userId, Fecha = hoy.AddDays(1), PuntajeTotal = 100 }
            );
            _ = await _context.SaveChangesAsync();

            // Act
            var result = (await _service.ObtenerCuestionariosAsync(userId)).ToList();

            // Assert
            Assert.Equal(100, result[0].PuntajeTotal);
            Assert.Equal(50, result[1].PuntajeTotal);
        }

        [Fact]
        public async Task ObtenerCuestionariosAsync_DeberiaMapearRespuestasCorrectamente()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var cuestionario = new Cuestionario 
            { 
                IdUsuario = userId, 
                Respuestas = new List<RespuestaCuestionario> 
                { 
                    new RespuestaCuestionario { Pregunta = "Q1", Valor = "V1", Puntaje = 5 } 
                } 
            };
            _ = _context.Cuestionarios.Add(cuestionario);
            _ = await _context.SaveChangesAsync();

            // Act
            var result = await _service.ObtenerCuestionariosAsync(userId);

            // Assert
            var dto = result.First();
            Assert.NotNull(dto.Respuestas);
            _ = Assert.Single(dto.Respuestas);
            Assert.Equal("Q1", dto.Respuestas[0].Pregunta);
            Assert.Equal("V1", dto.Respuestas[0].Valor);
        }

        [Fact]
        public async Task CrearCuestionarioAsync_DeberiaLanzarArgumentException_CuandoCompletadoEsNull()
        {
            // Arrange
            var dto = new CuestionarioDto { Completado = null };

            // Act & Assert
            _ = await Assert.ThrowsAsync<ArgumentException>(() => _service.CrearCuestionarioAsync(Guid.NewGuid(), dto));
        }

        [Fact]
        public async Task CrearCuestionarioAsync_DeberiaGuardarEnBdyRetornarDto_CuandoEsValido()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new CuestionarioDto
            {
                PuntajeTotal = 85,
                Completado = true,
                Perfil = "Enfocado",
                Respuestas = new List<RespuestaDto>
                {
                    new RespuestaDto { Pregunta = "P1", Valor = "A1", Categoria = "Foco", Puntaje = 10 }
                }
            };

            // Act
            var result = await _service.CrearCuestionarioAsync(userId, dto);

            // Assert
            Assert.NotEqual(0, result.IdCuestionario);
            Assert.Equal(85, result.PuntajeTotal);
            Assert.NotNull(result.Respuestas);
            _ = Assert.Single(result.Respuestas);
            Assert.Equal("P1", result.Respuestas![0].Pregunta);

            var entity = await _context.Cuestionarios.Include(c => c.Respuestas)
                .FirstOrDefaultAsync(c => c.IdCuestionario == result.IdCuestionario);
            Assert.NotNull(entity);
            Assert.Equal(userId, entity.IdUsuario);
            Assert.Equal(DateTimeKind.Utc, entity.Fecha.Kind);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _ = _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}