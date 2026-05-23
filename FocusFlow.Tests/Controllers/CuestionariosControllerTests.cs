using FocusFlowAPI.Controllers;
using FocusFlowAPI.DTOs;
using FocusFlowAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace FocusFlow.Tests.Controllers
{
    public class CuestionariosControllerTests
    {
        private readonly Mock<ICuestionarioService> _mockService;
        private readonly CuestionariosController _controller;

        public CuestionariosControllerTests()
        {
            _mockService = new Mock<ICuestionarioService>();
            _controller = new CuestionariosController(_mockService.Object);
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
        public async Task GetCuestionarios_DeberiaRetornarOk_CuandoUsuarioEstaAutenticado()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var cuestionariosEsperados = new List<CuestionarioDto> 
            { 
                new CuestionarioDto { IdCuestionario = 1, PuntajeTotal = 80, Completado = true } 
            };

            _mockService.Setup(s => s.ObtenerCuestionariosAsync(userId))
                .ReturnsAsync(cuestionariosEsperados);

            // Act
            var resultado = await _controller.GetCuestionarios();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(cuestionariosEsperados, okResult.Value);
        }

        [Fact]
        public async Task GetCuestionarios_DeberiaRetornarUnauthorized_CuandoNoHayIdUsuarioEnToken()
        {
            // Arrange
            SetupUser(null);

            // Act
            var resultado = await _controller.GetCuestionarios();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(resultado);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task CrearCuestionario_DeberiaRetornarOk_CuandoRegistroEsExitoso()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var dtoInput = new CuestionarioDto { PuntajeTotal = 50, Completado = true };
            var dtoOutput = new CuestionarioDto { IdCuestionario = 10, PuntajeTotal = 50, Completado = true };

            _mockService.Setup(s => s.CrearCuestionarioAsync(userId, dtoInput))
                .ReturnsAsync(dtoOutput);

            // Act
            var resultado = await _controller.CrearCuestionario(dtoInput);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(dtoOutput, okResult.Value);
        }
    }
}