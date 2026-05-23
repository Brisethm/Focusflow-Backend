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
    public class SesionesEnfoqueControllerTests
    {
        private readonly Mock<ISesionEnfoqueService> _mockService;
        private readonly SesionesEnfoqueController _controller;
        private const string InvalidUserIdMessage = "El token no contiene un identificador de usuario válido.";

        public SesionesEnfoqueControllerTests()
        {
            _mockService = new Mock<ISesionEnfoqueService>();
            _controller = new SesionesEnfoqueController(_mockService.Object);
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
        public async Task GetSesiones_DeberiaRetornarOk_CuandoUsuarioEstaAutenticado()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var sesiones = new List<SesionEnfoqueDto> { new SesionEnfoqueDto { IdSesion = 1, Tipo = "pomodoro", DuracionMinutos = 25 } };
            _mockService.Setup(s => s.ObtenerSesionesAsync(userId)).ReturnsAsync(sesiones);

            var result = await _controller.GetSesiones();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(sesiones, okResult.Value);
        }

        [Fact]
        public async Task GetSesiones_DeberiaRetornarUnauthorized_CuandoNoHayIdUsuario()
        {
            SetupUser(null);
            var result = await _controller.GetSesiones();
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(InvalidUserIdMessage, unauthorizedResult.Value);
        }

        [Fact]
        public async Task GetSesion_DeberiaRetornarOk_CuandoSesionExiste()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var sesion = new SesionEnfoqueDto { IdSesion = 1, Tipo = "pomodoro", DuracionMinutos = 25 };
            _mockService.Setup(s => s.ObtenerSesionPorIdAsync(userId, 1)).ReturnsAsync(sesion);

            var result = await _controller.GetSesion(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(sesion, okResult.Value);
        }

        [Fact]
        public async Task GetSesion_DeberiaRetornarNotFound_CuandoNoExiste()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.ObtenerSesionPorIdAsync(userId, 1)).ReturnsAsync((SesionEnfoqueDto?)null);

            var result = await _controller.GetSesion(1);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CrearSesion_DeberiaRetornarOk_CuandoEsExitoso()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var dto = new SesionEnfoqueDto { DuracionMinutos = 25, Tipo = "work" };
            _mockService.Setup(s => s.CrearSesionAsync(userId, dto)).ReturnsAsync(new SesionEnfoqueDto { IdSesion = 1, DuracionMinutos = 25, Tipo = "work" });

            var result = await _controller.CrearSesion(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<SesionEnfoqueDto>(okResult.Value);
            Assert.Equal(1, returned.IdSesion);
        }

        [Fact]
        public async Task ActualizarSesion_DeberiaRetornarNotFound_CuandoServiceDevuelveNull()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.ActualizarSesionAsync(userId, 1, It.IsAny<SesionEnfoqueDto>()))
                .ReturnsAsync((SesionEnfoqueDto?)null);

            var result = await _controller.ActualizarSesion(1, new SesionEnfoqueDto { Tipo = "any", DuracionMinutos = 1 });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task EliminarSesion_DeberiaRetornarNoContent_CuandoEsExitoso()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.EliminarSesionAsync(userId, 1)).ReturnsAsync(true);

            var result = await _controller.EliminarSesion(1);

            Assert.IsType<NoContentResult>(result);
        }
    }
}