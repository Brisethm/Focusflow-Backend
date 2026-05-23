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
    public class RegistrosEmocionalesControllerTests
    {
        private readonly Mock<IRegistroEmocionalService> _mockService;
        private readonly RegistrosEmocionalesController _controller;
        private const string ErrorTokenInvalido = "El token no contiene un identificador de usuario válido.";

        public RegistrosEmocionalesControllerTests()
        {
            _mockService = new Mock<IRegistroEmocionalService>();
            _controller = new RegistrosEmocionalesController(_mockService.Object);
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
        public async Task GetRegistros_DeberiaRetornarOk_CuandoUsuarioEstaAutenticado()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var registros = new List<RegistroEmocionalResponseDto> { new RegistroEmocionalResponseDto { IdRegistro = 1, EstadoAnimo = "Feliz" } };
            _mockService.Setup(s => s.ObtenerRegistrosAsync(userId)).ReturnsAsync(registros);

            // Act
            var result = await _controller.GetRegistros();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(registros, okResult.Value);
        }

        [Fact]
        public async Task GetRegistros_DeberiaRetornarUnauthorized_CuandoNoHayUserId()
        {
            // Arrange
            SetupUser(null);

            // Act
            var result = await _controller.GetRegistros();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(ErrorTokenInvalido, unauthorizedResult.Value);
        }

        [Fact]
        public async Task GetRegistro_DeberiaRetornarOk_CuandoExiste()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var response = new RegistroEmocionalResponseDto { IdRegistro = 1, EstadoAnimo = "Tranquilo" };
            _mockService.Setup(s => s.ObtenerRegistroPorIdAsync(userId, 1)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetRegistro(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task CrearRegistro_DeberiaRetornarOk_CuandoEsExitoso()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var input = new RegistroEmocionalDto { EstadoAnimo = "Motivado", NivelEnergia = 8 };
            var output = new RegistroEmocionalResponseDto { IdRegistro = 10, EstadoAnimo = "Motivado" };
            _mockService.Setup(s => s.CrearRegistroAsync(userId, input)).ReturnsAsync(output);

            // Act
            var result = await _controller.CrearRegistro(input);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(output, okResult.Value);
        }

        [Fact]
        public async Task ActualizarRegistro_DeberiaRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.ActualizarRegistroAsync(userId, 1, It.IsAny<RegistroEmocionalDto>()))
                .ReturnsAsync((RegistroEmocionalResponseDto?)null);

            // Act
            var result = await _controller.ActualizarRegistro(1, new RegistroEmocionalDto());

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task EliminarRegistro_DeberiaRetornarNoContent_CuandoEsExitoso()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.EliminarRegistroAsync(userId, 1)).ReturnsAsync(true);

            // Act
            var result = await _controller.EliminarRegistro(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}