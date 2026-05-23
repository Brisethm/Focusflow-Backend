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
    public class TransaccionesControllerTests
    {
        private readonly Mock<ITransaccionService> _mockService;
        private readonly TransaccionesController _controller;
        private const string ErrorTokenInvalido = "El token no contiene un identificador de usuario válido.";

        public TransaccionesControllerTests()
        {
            _mockService = new Mock<ITransaccionService>();
            _controller = new TransaccionesController(_mockService.Object);
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
        public async Task GetTransacciones_DeberiaRetornarOk_CuandoUsuarioEstaAutenticado()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);

            var transacciones = new List<TransaccionDto>
            {
                new() { IdTransaccion = 1, Monto = 100, Tipo = "Ingreso", Categoria = "Sueldo" }
            };
            _mockService.Setup(s => s.ObtenerTransaccionesAsync(userId)).ReturnsAsync(transacciones);

            // Act
            var result = await _controller.GetTransacciones();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(transacciones, okResult.Value);
        }

        [Fact]
        public async Task GetTransacciones_DeberiaRetornarUnauthorized_CuandoNoHayUserId()
        {
            // Arrange
            SetupUser(null);

            // Act
            var result = await _controller.GetTransacciones();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(ErrorTokenInvalido, unauthorizedResult.Value);
        }

        [Fact]
        public async Task GetTransaccion_DeberiaRetornarOk_CuandoExiste()
        {
            // Arrange
            var idUsuario = Guid.NewGuid();
            SetupUser(idUsuario);

            var response = new TransaccionDto { IdTransaccion = 1, Monto = 50, Tipo = "Gasto", Categoria = "Ocio" };
            _mockService.Setup(s => s.ObtenerTransaccionPorIdAsync(idUsuario, 1)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetTransaccion(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task CrearTransaccion_DeberiaRetornarOk_CuandoEsExitoso()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);

            var dto = new TransaccionDto { Monto = 20, Tipo = "Gasto", Categoria = "Comida" };
            _mockService.Setup(s => s.CrearTransaccionAsync(userId, dto)).ReturnsAsync(dto);

            // Act
            var result = await _controller.CrearTransaccion(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(dto, okResult.Value);
        }

        [Fact]
        public async Task ActualizarTransaccion_DeberiaRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.ActualizarTransaccionAsync(userId, 1, It.IsAny<TransaccionDto>()))
                .ReturnsAsync((TransaccionDto?)null);

            // Act

            var dtoDummy = new TransaccionDto { Monto = 0, Tipo = "Dummy", Categoria = "Dummy" };
            var result = await _controller.ActualizarTransaccion(1, dtoDummy);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task EliminarTransaccion_DeberiaRetornarNoContent_CuandoEsExitoso()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.EliminarTransaccionAsync(userId, 1)).ReturnsAsync(true);

            // Act
            var result = await _controller.EliminarTransaccion(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}