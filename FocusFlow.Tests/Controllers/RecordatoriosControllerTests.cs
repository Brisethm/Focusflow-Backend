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
    public class RecordatoriosControllerTests
    {
        private readonly Mock<IRecordatorioService> _mockService;
        private readonly RecordatoriosController _controller;
        private const string InvalidUserIdMessage = "El token no contiene un identificador de usuario valido.";

        public RecordatoriosControllerTests()
        {
            _mockService = new Mock<IRecordatorioService>();
            _controller = new RecordatoriosController(_mockService.Object);
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
        public async Task GetRecordatorios_DeberiaRetornarOk_CuandoUsuarioEstaAutenticado()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var recordatorios = new List<RecordatorioDto> { new RecordatorioDto { IdRecordatorio = 1, Mensaje = "Test" } };
            _mockService.Setup(s => s.ObtenerRecordatoriosAsync(userId)).ReturnsAsync(recordatorios);

            var result = await _controller.GetRecordatorios();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(recordatorios, okResult.Value);
        }

        [Fact]
        public async Task GetRecordatorios_DeberiaRetornarUnauthorized_CuandoNoHayIdUsuario()
        {
            SetupUser(null);
            var result = await _controller.GetRecordatorios();
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(InvalidUserIdMessage, unauthorizedResult.Value);
        }

        [Fact]
        public async Task GetRecordatorio_DeberiaRetornarNotFound_CuandoNoExiste()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.ObtenerRecordatorioPorIdAsync(userId, 1)).ReturnsAsync((RecordatorioDto?)null);

            var result = await _controller.GetRecordatorio(1);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CrearRecordatorio_DeberiaRetornarOk_CuandoEsExitoso()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var dto = new RecordatorioDto { Mensaje = "Nuevo" };
            _mockService.Setup(s => s.CrearRecordatorioAsync(userId, dto)).ReturnsAsync(new RecordatorioDto { IdRecordatorio = 10, Mensaje = "Nuevo" });

            var result = await _controller.CrearRecordatorio(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<RecordatorioDto>(okResult.Value);
            Assert.Equal(10, returned.IdRecordatorio);
        }

        [Fact]
        public async Task ActualizarRecordatorio_DeberiaRetornarNotFound_CuandoServiceDevuelveNull()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.ActualizarRecordatorioAsync(userId, 1, It.IsAny<RecordatorioDto>()))
                .ReturnsAsync((RecordatorioDto?)null);

            var result = await _controller.ActualizarRecordatorio(1, new RecordatorioDto { Mensaje = string.Empty });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task EliminarRecordatorio_DeberiaRetornarNoContent_CuandoEsExitoso()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.EliminarRecordatorioAsync(userId, 1)).ReturnsAsync(true);

            var result = await _controller.EliminarRecordatorio(1);

            Assert.IsType<NoContentResult>(result);
        }
    }
}