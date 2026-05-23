using FocusFlowAPI.Controllers;
using FocusFlowAPI.DTOs;
using FocusFlowAPI.Hubs;
using FocusFlowAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System.Security.Claims;
using Xunit;

namespace FocusFlow.Tests.Controllers
{
    public class TicketsControllerTests
    {
        private readonly Mock<ITicketService> _mockService;
        private readonly Mock<IHubContext<TicketHub>> _mockHubContext;
        private readonly TicketsController _controller;

        public TicketsControllerTests()
        {
            _mockService = new Mock<ITareaService>().As<ITicketService>();
            _mockService = new Mock<ITicketService>();
            _mockHubContext = new Mock<IHubContext<TicketHub>>();
            _controller = new TicketsController(_mockService.Object, _mockHubContext.Object);
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
        public async Task GetAll_DeberiaRetornarForbidden_CuandoUsuarioNoEsStaff()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.IsStaffAsync(userId)).ReturnsAsync(false);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, objectResult.StatusCode);
        }

        [Fact]
        public async Task SendResponse_DeberiaNotificarViaSignalR_CuandoRespuestaEsExitosa()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var idTicket = 123;
            var dto = new CreateRespuestaDto { Mensaje = "Respuesta de prueba" };
            var respuestaDto = new TicketRespuestaDto { IdRespuesta = 1, Mensaje = dto.Mensaje, IdAutor = userId };

            _mockService.Setup(s => s.AddResponse(idTicket, userId, dto.Mensaje))
                .ReturnsAsync(respuestaDto);

            // Mocks para SignalR
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.Group(idTicket.ToString())).Returns(mockClientProxy.Object);

            // Act
            var result = await _controller.SendResponse(idTicket, dto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            mockClientProxy.Verify(
                p => p.SendCoreAsync("ReceiveMessage", It.Is<object[]>(o => o[0] == respuestaDto), default),
                Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_DeberiaRetornarNotFound_SiTicketNoExiste()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.IsStaffAsync(userId)).ReturnsAsync(true);
            _mockService.Setup(s => s.UpdateTicketStatus(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateStatus(1, "closed");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}