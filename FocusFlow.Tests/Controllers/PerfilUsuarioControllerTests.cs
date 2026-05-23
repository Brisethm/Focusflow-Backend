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
    public class PerfilUsuarioControllerTests
    {
        private readonly Mock<IPerfilUsuarioService> _mockService;
        private readonly PerfilUsuarioController _controller;
        private const string InvalidUserIdTokenMessage = "El token no contiene un identificador de usuario válido.";

        public PerfilUsuarioControllerTests()
        {
            _mockService = new Mock<IPerfilUsuarioService>();
            _controller = new PerfilUsuarioController(_mockService.Object);
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
        public async Task GetPerfil_DeberiaRetornarOk_CuandoPerfilExiste()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var perfilEsperado = new PerfilUsuarioDto { IdUsuario = userId, Nombre = "Test User" };
            _mockService.Setup(s => s.ObtenerPerfilAsync(userId)).ReturnsAsync(perfilEsperado);

            // Act
            var result = await _controller.GetPerfil();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(perfilEsperado, okResult.Value);
        }

        [Fact]
        public async Task GetPerfil_DeberiaRetornarNotFound_CuandoPerfilNoExiste()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.ObtenerPerfilAsync(userId)).ReturnsAsync((PerfilUsuarioDto?)null);

            // Act
            var result = await _controller.GetPerfil();

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CrearPerfil_DeberiaLlamarServicioYRetornarOk()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var inputDto = new PerfilUsuarioDto { Nombre = "New User" };
            var outputDto = new PerfilUsuarioDto { IdUsuario = userId, Nombre = "New User", Rol = "user" };
            
            _mockService.Setup(s => s.CrearPerfilAsync(userId, It.IsAny<PerfilUsuarioDto>()))
                .ReturnsAsync(outputDto);

            // Act
            var result = await _controller.CrearPerfil(inputDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(outputDto, okResult.Value);
            _mockService.Verify(s => s.CrearPerfilAsync(userId, It.Is<PerfilUsuarioDto>(d => d.Rol == "user")), Times.Once);
        }

        [Fact]
        public async Task EliminarPerfil_DeberiaRetornarNoContent_CuandoSeEliminaExitosamente()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.EliminarPerfilAsync(userId)).ReturnsAsync(true);

            // Act
            var result = await _controller.EliminarPerfil();

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetPerfil_DeberiaRetornarUnauthorized_CuandoNoHayUserId()
        {
            // Arrange
            SetupUser(null);

            // Act
            var result = await _controller.GetPerfil();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(InvalidUserIdTokenMessage, unauthorizedResult.Value);
        }
    }
}