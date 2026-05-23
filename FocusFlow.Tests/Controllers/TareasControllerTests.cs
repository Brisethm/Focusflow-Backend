using FocusFlowAPI.Controllers;
using FocusFlowAPI.DTOs;
using FocusFlowAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace FocusFlow.Tests.Controllers
{
    public class TareasControllerTests
    {
        private readonly Mock<ITareaService> _mockService;
        private readonly TareasController _controller;

        public TareasControllerTests()
        {
            _mockService = new Mock<ITareaService>();
            _controller = new TareasController(_mockService.Object);
        }

        private void SetupUser(Guid? userId)
        {
            var claims = new List<Claim>();
            if (userId.HasValue)
            {
                claims.Add(new Claim("sub", userId.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task GetTareas_DeberiaRetornarOk_CuandoUsuarioEstaAutenticado()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var tareas = new List<TareaDto> { new TareaDto { IdTarea = 1, Titulo = "Test", Prioridad = "Alta", NivelEsfuerzo = "Medio", Estado = "Pendiente" } };
            _mockService.Setup(s => s.ObtenerTareasAsync(userId)).ReturnsAsync(tareas);

            var result = await _controller.GetTareas();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(tareas, okResult.Value);
        }

        [Fact]
        public async Task GetTareas_DeberiaRetornarUnauthorized_CuandoNoHayUserId()
        {
            SetupUser(null);
            var result = await _controller.GetTareas();
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetTarea_DeberiaRetornarOk_CuandoTareaExiste()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var tarea = new TareaDto { IdTarea = 1, Titulo = "Test", Prioridad = "Alta", NivelEsfuerzo = "Medio", Estado = "Pendiente" };
            _mockService.Setup(s => s.ObtenerTareaPorIdAsync(userId, 1)).ReturnsAsync(tarea);

            var result = await _controller.GetTarea(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(tarea, okResult.Value);
        }

        [Fact]
        public async Task GetTarea_DeberiaRetornarNotFound_CuandoNoExiste()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.ObtenerTareaPorIdAsync(userId, 1)).ReturnsAsync((TareaDto?)null);

            var result = await _controller.GetTarea(1);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CrearTarea_DeberiaRetornarOk_CuandoEsExitoso()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var request = new TareaRequestDto { Titulo = "Nueva", Prioridad = "Baja", NivelEsfuerzo = "Bajo", Estado = "Pendiente" };
            var response = new TareaDto { IdTarea = 10, Titulo = "Nueva", Prioridad = "Baja", NivelEsfuerzo = "Bajo", Estado = "Pendiente" };
            _mockService.Setup(s => s.CrearTareaAsync(userId, request)).ReturnsAsync(response);

            var result = await _controller.CrearTarea(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task ActualizarTarea_DeberiaRetornarNotFound_CuandoServiceDevuelveNull()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.ActualizarTareaAsync(userId, 1, It.IsAny<TareaRequestDto>()))
                .ReturnsAsync((TareaDto?)null);

            var result = await _controller.ActualizarTarea(1, new TareaRequestDto { Titulo = "Update", Prioridad = "A", NivelEsfuerzo = "B", Estado = "C" });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task EliminarTarea_DeberiaRetornarNoContent_CuandoEsExitoso()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.EliminarTareaAsync(userId, 1)).ReturnsAsync(true);

            var result = await _controller.EliminarTarea(1);

            Assert.IsType<NoContentResult>(result);
        }
    }
}