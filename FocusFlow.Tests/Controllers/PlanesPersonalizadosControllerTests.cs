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
    public class PlanesPersonalizadosControllerTests
    {
        private readonly Mock<IPlanPersonalizadoService> _mockService;
        private readonly PlanesPersonalizadosController _controller;

        public PlanesPersonalizadosControllerTests()
        {
            _mockService = new Mock<IPlanPersonalizadoService>();
            _controller = new PlanesPersonalizadosController(_mockService.Object);
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
        public async Task GetPlanes_DeberiaRetornarOk_CuandoUsuarioEstaAutenticado()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var planesEsperados = new List<PlanPersonalizadoDto> { new PlanPersonalizadoDto { IdPlan = 1 } };
            _mockService.Setup(s => s.ObtenerPlanesAsync(userId)).ReturnsAsync(planesEsperados);

            // Act
            var result = await _controller.GetPlanes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(planesEsperados, okResult.Value);
        }

        [Fact]
        public async Task GetPlanes_DeberiaRetornarUnauthorized_CuandoNoHayUserId()
        {
            // Arrange
            SetupUser(null);

            // Act
            var result = await _controller.GetPlanes();

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetPlan_DeberiaRetornarNotFound_CuandoPlanNoExiste()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.ObtenerPlanPorIdAsync(userId, 1)).ReturnsAsync((PlanPersonalizadoDto?)null);

            // Act
            var result = await _controller.GetPlan(1);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CrearPlan_DeberiaRetornarCreated_CuandoEsExitoso()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var dto = new CreatePlanDto { EnfoqueDiario = 5 };
            var createdPlan = new PlanPersonalizadoDto { IdPlan = 10, EnfoqueDiario = 5 };
            _mockService.Setup(s => s.CrearPlanAsync(userId, dto)).ReturnsAsync(createdPlan);

            // Act
            var result = await _controller.CrearPlan(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(createdPlan.IdPlan, createdResult.RouteValues!["idPlan"]);
            Assert.Equal(createdPlan, createdResult.Value);
        }

        [Fact]
        public async Task CrearPlan_DeberiaRetornarConflict_CuandoLanzaInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.CrearPlanAsync(userId, It.IsAny<CreatePlanDto>()))
                .ThrowsAsync(new InvalidOperationException("Ya existe un plan"));

            // Act
            var result = await _controller.CrearPlan(new CreatePlanDto());

            // Assert
            var conflictResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, conflictResult.StatusCode);
            Assert.Equal("Ya existe un plan", conflictResult.Value);
        }

        [Fact]
        public async Task EliminarPlan_DeberiaRetornarNoContent_CuandoSeEliminaCorrectamente()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.EliminarPlanAsync(userId, 1)).ReturnsAsync(true);

            // Act
            var result = await _controller.EliminarPlan(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}