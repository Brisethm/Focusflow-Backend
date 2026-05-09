using Microsoft.EntityFrameworkCore;
using FocusFlowAPI.Models;
using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Services
{
    public class TicketService
    {
        private readonly UsuarioContext _context;

        public TicketService(UsuarioContext context)
        {
            _context = context;
        }

        #region Métodos de Lectura
        public async Task<List<TicketDto>> GetUserTickets(Guid userId)
        {
            return await _context.Tickets
                .Where(t => t.IdUsuario == userId)
                .OrderByDescending(t => t.FechaCreacion)
                .Select(t => MapToDto(t))
                .ToListAsync();
        }
        public async Task<bool> IsStaffAsync(Guid userId)
        {
            var perfil = await _context.PerfilUsuarios.FirstOrDefaultAsync(p => p.IdUsuario == userId);
            return perfil != null && (perfil.Rol == "admin" || perfil.Rol == "support");
        }
        public async Task<List<TicketDto>> GetAllTickets()
        {
            return await _context.Tickets
                .OrderByDescending(t => t.Prioridad)
                .Select(t => MapToDto(t))
                .ToListAsync();
        }
        public async Task<List<TicketRespuestaDto>> GetTicketResponses(int ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return new List<TicketRespuestaDto>();

            return await _context.RespuestasTickets
                .Where(r => r.IdTicket == ticketId)
                .OrderBy(r => r.Fecha)
                .Select(r => new TicketRespuestaDto
                {
                    IdRespuesta = r.IdRespuesta,
                    IdAutor = r.IdAutor,
                    Mensaje = r.Mensaje,
                    Fecha = r.Fecha,
                    // Marcamos como soporte si el autor no es quien creó el ticket
                    EsSoporte = r.IdAutor != ticket.IdUsuario
                })
                .ToListAsync();
        }

        #endregion

        #region Métodos de Escritura (Usuario)
        public async Task<TicketDto> CreateTicket(Guid userId, CreateTicketDto dto)
        {
            var ticket = new Ticket
            {
                IdUsuario = userId,
                Asunto = dto.Asunto,
                Descripcion = dto.Descripcion,
                Categoria = dto.Categoria,
                Prioridad = dto.Prioridad,
                Estado = "open",
                FechaCreacion = DateTime.UtcNow,
                UltimaActualizacion = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return MapToDto(ticket);
        }

        public async Task<bool> CancelTicket(int ticketId, Guid userId)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.IdTicket == ticketId && t.IdUsuario == userId);

            if (ticket == null) return false;

            ticket.Estado = "closed";
            ticket.UltimaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Métodos de Soporte / Interacción
        public async Task<TicketRespuestaDto> AddResponse(int ticketId, Guid userId, string mensaje)
        {
            var respuesta = new TicketRespuesta
            {
                IdTicket = ticketId,
                IdAutor = userId,
                Mensaje = mensaje,
                Fecha = DateTime.UtcNow
            };

            _context.RespuestasTickets.Add(respuesta);

            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket != null)
            {
                if (ticket.IdUsuario != userId)
                {
                    ticket.Estado = "in_progress";
                }
                ticket.UltimaActualizacion = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new TicketRespuestaDto
            {
                IdRespuesta = respuesta.IdRespuesta,
                IdAutor = userId,
                Mensaje = mensaje,
                Fecha = respuesta.Fecha,
                EsSoporte = ticket != null && userId != ticket.IdUsuario
            };
        }
        public async Task<bool> UpdateTicketStatus(int ticketId, string newStatus, Guid? assignedTo = null)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return false;

            ticket.Estado = newStatus;
            if (assignedTo.HasValue) ticket.IdAsignado = assignedTo;

            ticket.UltimaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        private static TicketDto MapToDto(Ticket t) => new TicketDto
        {
            IdTicket = t.IdTicket,
            Asunto = t.Asunto,
            Descripcion = t.Descripcion,
            Estado = t.Estado,
            Prioridad = t.Prioridad,
            Categoria = t.Categoria,
            IdAsignado = t.IdAsignado,
            FechaCreacion = t.FechaCreacion
        };
    }
}