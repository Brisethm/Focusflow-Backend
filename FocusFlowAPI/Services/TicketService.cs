using Microsoft.EntityFrameworkCore;
using FocusFlowAPI.Models;
using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Services
{
    public class TicketService : ITicketService
    {
        private readonly UsuarioContext _context;

        public TicketService(UsuarioContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Métodos de Lectura

        public async Task<IEnumerable<TicketDto>> GetUserTickets(Guid idUsuario)
        {
            return await _context.Tickets
                .AsNoTracking()
                .Where(t => t.IdUsuario == idUsuario)
                .OrderByDescending(t => t.FechaCreacion)
                .Select(t => new TicketDto
                {
                    IdTicket = t.IdTicket,
                    Asunto = t.Asunto,
                    Descripcion = t.Descripcion,
                    Estado = t.Estado,
                    Prioridad = t.Prioridad,
                    Categoria = t.Categoria,
                    IdAsignado = t.IdAsignado,
                    FechaCreacion = t.FechaCreacion
                })
                .ToListAsync();
        }

        public async Task<bool> IsStaffAsync(Guid idUsuario)
        {
            var perfil = await _context.PerfilUsuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdUsuario == idUsuario);

            return perfil is not null && (perfil.Rol is "admin" || perfil.Rol is "support");
        }

        public async Task<IEnumerable<TicketDto>> GetAllTickets()
        {
            return await _context.Tickets
                .AsNoTracking()
                .OrderByDescending(t => t.Prioridad)
                .Select(t => new TicketDto
                {
                    IdTicket = t.IdTicket,
                    Asunto = t.Asunto,
                    Descripcion = t.Descripcion,
                    Estado = t.Estado,
                    Prioridad = t.Prioridad,
                    Categoria = t.Categoria,
                    IdAsignado = t.IdAsignado,
                    FechaCreacion = t.FechaCreacion
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<TicketRespuestaDto>> GetTicketResponses(int idTicket)
        {
            var ticket = await _context.Tickets.FindAsync(idTicket);
            if (ticket is null) 
            {
                return [];
            }

            return await _context.RespuestasTickets
                .AsNoTracking()
                .Where(r => r.IdTicket == idTicket)
                .OrderBy(r => r.Fecha)
                .Select(r => new TicketRespuestaDto
                {
                    IdRespuesta = r.IdRespuesta,
                    IdAutor = r.IdAutor,
                    Mensaje = r.Mensaje,
                    Fecha = r.Fecha,
                    EsSoporte = r.IdAutor != ticket.IdUsuario
                })
                .ToListAsync();
        }

        #endregion

        #region Métodos de Escritura (Usuario)

        public async Task<TicketDto> CreateTicket(Guid idUsuario, CreateTicketDto dto)
        {
            var ticket = new Ticket
            {
                IdUsuario = idUsuario,
                Asunto = dto.Asunto,
                Descripcion = dto.Descripcion,
                Categoria = dto.Categoria,
                Prioridad = dto.Prioridad,
                Estado = "open",
                FechaCreacion = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                UltimaActualizacion = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return MapToDto(ticket);
        }

        public async Task<bool> CancelTicket(int idTicket, Guid idUsuario)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.IdTicket == idTicket && t.IdUsuario == idUsuario);

            if (ticket is null) 
            {
                return false;
            }

            ticket.Estado = "closed";
            ticket.UltimaActualizacion = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Métodos de Soporte / Interacción

        public async Task<TicketRespuestaDto?> AddResponse(int idTicket, Guid idUsuario, string mensaje)
        {
            var respuesta = new TicketRespuesta
            {
                IdTicket = idTicket,
                IdAutor = idUsuario,
                Mensaje = mensaje,
                Fecha = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };

            _context.RespuestasTickets.Add(respuesta);

            var ticket = await _context.Tickets.FindAsync(idTicket);
            if (ticket is not null)
            {
                if (ticket.IdUsuario != idUsuario)
                {
                    ticket.Estado = "in_progress";
                }
                ticket.UltimaActualizacion = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            }

            await _context.SaveChangesAsync();

            return new TicketRespuestaDto
            {
                IdRespuesta = respuesta.IdRespuesta,
                IdAutor = idUsuario,
                Mensaje = mensaje,
                Fecha = respuesta.Fecha,
                EsSoporte = ticket is not null && idUsuario != ticket.IdUsuario
            };
        }

        public async Task<bool> UpdateTicketStatus(int idTicket, string nuevoEstado)
        {
            var ticket = await _context.Tickets.FindAsync(idTicket);
            if (ticket is null) 
            {
                return false;
            }

            ticket.Estado = nuevoEstado;
            ticket.UltimaActualizacion = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        private static TicketDto MapToDto(Ticket t) =>
            new()
            {
                IdTicket = t.IdTicket,
                Asunto = t.Asunto,
                Descripcion = t.Descripcion,
                Estado = t.Estado,
                Prioridad = t.Prioridad,
                Categoria = t.Categoria,
                IdAsignado = t.IdAsignado,
                FechaCreacion = DateTime.SpecifyKind(t.FechaCreacion, DateTimeKind.Utc)
            };
    }
}