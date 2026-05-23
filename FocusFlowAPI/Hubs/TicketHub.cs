using Microsoft.AspNetCore.SignalR;

namespace FocusFlowAPI.Hubs
{
    public class TicketHub : Hub
    {
        public async Task JoinTicketGroup(string ticketId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ticketId);
        }
    }
}