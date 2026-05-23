using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;
using FocusFlowAPI.Hubs;

namespace FocusFlowAPI.Tests.Hubs
{
    public class TicketHubTests
    {
        [Fact]
        public async Task JoinTicketGroup_DeberiaAgregarConexionAlGrupo_CuandoSeLlamaConUnTicketId()
        {
            // Arrange
            var connectionId = "conexion-simulada-123";
            var ticketId = "ticket-test-789";

            var mockContext = new Mock<HubCallerContext>();
            mockContext.Setup(c => c.ConnectionId).Returns(connectionId);

            var mockGroups = new Mock<IGroupManager>();

            var hub = new TicketHub
            {
                Context = mockContext.Object,
                Groups = mockGroups.Object
            };

            // Act
            await hub.JoinTicketGroup(ticketId);

            // Assert
            mockGroups.Verify(g => g.AddToGroupAsync(
                connectionId,
                ticketId,
                It.IsAny<CancellationToken>()),
            Times.Once);
        }
    }
}