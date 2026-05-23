using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Services
{
    public interface ITicketService
    {
        Task<IEnumerable<TicketDto>> GetUserTickets(Guid idUsuario);
        Task<TicketDto> CreateTicket(Guid idUsuario, CreateTicketDto dto);
        Task<bool> IsStaffAsync(Guid idUsuario);
        Task<IEnumerable<TicketDto>> GetAllTickets();
        Task<IEnumerable<TicketRespuestaDto>> GetTicketResponses(int idTicket);
        Task<TicketRespuestaDto?> AddResponse(int idTicket, Guid idUsuario, string mensaje);
        Task<bool> UpdateTicketStatus(int idTicket, string nuevoEstado);
        Task<bool> CancelTicket(int idTicket, Guid idUsuario);
    }
}