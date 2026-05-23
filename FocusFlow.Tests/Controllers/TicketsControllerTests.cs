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
        public async Task GetMyTickets_DeberiaRetornarUnauthorized_CuandoTokenNoTieneUsuario()
        {
            SetupUser(null);

            var result = await _controller.GetMyTickets();

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetMyTickets_DeberiaRetornarOk_ConTicketsDelUsuario()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var tickets = new List<TicketDto>
            {
                new() { IdTicket = 1, Asunto = "Ayuda", Estado = "open" }
            };
            _mockService.Setup(s => s.GetUserTickets(userId)).ReturnsAsync(tickets);

            var result = await _controller.GetMyTickets();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(tickets, ok.Value);
        }

        [Fact]
        public async Task CreateTicket_DeberiaRetornarUnauthorized_CuandoTokenNoTieneUsuario()
        {
            SetupUser(null);

            var result = await _controller.CreateTicket(new CreateTicketDto());

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task CreateTicket_DeberiaRetornarOk_CuandoServicioCreaTicket()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var dto = new CreateTicketDto { Asunto = "Error", Descripcion = "Detalle" };
            var created = new TicketDto { IdTicket = 10, Asunto = dto.Asunto, Descripcion = dto.Descripcion };
            _mockService.Setup(s => s.CreateTicket(userId, dto)).ReturnsAsync(created);

            var result = await _controller.CreateTicket(dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(created, ok.Value);
        }

        [Fact]
        public async Task GetAll_DeberiaRetornarUnauthorized_CuandoTokenNoTieneUsuario()
        {
            SetupUser(null);

            var result = await _controller.GetAll();

            Assert.IsType<UnauthorizedObjectResult>(result);
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
        public async Task GetAll_DeberiaRetornarOk_CuandoUsuarioEsStaff()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            var tickets = new List<TicketDto>
            {
                new() { IdTicket = 1, Asunto = "Ticket" }
            };
            _mockService.Setup(s => s.IsStaffAsync(userId)).ReturnsAsync(true);
            _mockService.Setup(s => s.GetAllTickets()).ReturnsAsync(tickets);

            var result = await _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(tickets, ok.Value);
        }

        [Fact]
        public async Task GetResponses_DeberiaRetornarOk_ConRespuestas()
        {
            var respuestas = new List<TicketRespuestaDto>
            {
                new() { IdRespuesta = 1, Mensaje = "Hola" }
            };
            _mockService.Setup(s => s.GetTicketResponses(5)).ReturnsAsync(respuestas);

            var result = await _controller.GetResponses(5);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(respuestas, ok.Value);
        }

        [Fact]
        public async Task SendResponse_DeberiaRetornarUnauthorized_CuandoTokenNoTieneUsuario()
        {
            SetupUser(null);

            var result = await _controller.SendResponse(1, new CreateRespuestaDto { Mensaje = "Hola" });

            Assert.IsType<UnauthorizedObjectResult>(result);
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
        public async Task SendResponse_NoDebeNotificarViaSignalR_CuandoServicioRetornaNull()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.AddResponse(1, userId, "Hola"))
                .ReturnsAsync((TicketRespuestaDto?)null);

            var result = await _controller.SendResponse(1, new CreateRespuestaDto { Mensaje = "Hola" });

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Null(ok.Value);
            _mockHubContext.Verify(h => h.Clients, Times.Never);
        }

        [Fact]
        public async Task UpdateStatus_DeberiaRetornarUnauthorized_CuandoTokenNoTieneUsuario()
        {
            SetupUser(null);

            var result = await _controller.UpdateStatus(1, "closed");

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task UpdateStatus_DeberiaRetornarForbidden_CuandoUsuarioNoEsStaff()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.IsStaffAsync(userId)).ReturnsAsync(false);

            var result = await _controller.UpdateStatus(1, "closed");

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
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

        [Fact]
        public async Task UpdateStatus_DeberiaNotificarViaSignalR_CuandoActualizaEstado()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.IsStaffAsync(userId)).ReturnsAsync(true);
            _mockService.Setup(s => s.UpdateTicketStatus(2, "closed")).ReturnsAsync(true);

            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.Group("2")).Returns(mockClientProxy.Object);

            var result = await _controller.UpdateStatus(2, "closed");

            Assert.IsType<OkObjectResult>(result);
            mockClientProxy.Verify(
                p => p.SendCoreAsync("StatusUpdated", It.Is<object[]>(o => o.Length == 1), default),
                Times.Once);
        }

        [Fact]
        public async Task CancelTicket_DeberiaRetornarUnauthorized_CuandoTokenNoTieneUsuario()
        {
            SetupUser(null);

            var result = await _controller.CancelTicket(1);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task CancelTicket_DeberiaRetornarBadRequest_CuandoServicioRetornaFalse()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.CancelTicket(1, userId)).ReturnsAsync(false);

            var result = await _controller.CancelTicket(1);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CancelTicket_DeberiaRetornarOk_CuandoServicioCancelaTicket()
        {
            var userId = Guid.NewGuid();
            SetupUser(userId);
            _mockService.Setup(s => s.CancelTicket(1, userId)).ReturnsAsync(true);

            var result = await _controller.CancelTicket(1);

            Assert.IsType<OkObjectResult>(result);
        }
    }
}
