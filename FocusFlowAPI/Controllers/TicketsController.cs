using FocusFlowAPI.DTOs;
using FocusFlowAPI.Extensions;
using FocusFlowAPI.Services;
using FocusFlowAPI.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace FocusFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly IHubContext<TicketHub> _hubContext;
        private const string ErrorTokenInvalido = "El token no contiene un identificador de usuario válido.";

        public TicketsController(ITicketService ticketService, IHubContext<TicketHub> hubContext)
        {
            _ticketService = ticketService;
            _hubContext = hubContext;
        }

        [HttpGet("my-tickets")]
        [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyTickets()
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(ErrorTokenInvalido);
            }

            var tickets = await _ticketService.GetUserTickets(idUsuario.Value);
            return Ok(tickets);
        }

        [HttpPost]
        [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(ErrorTokenInvalido);
            }

            var result = await _ticketService.CreateTicket(idUsuario.Value, dto);
            return Ok(result);
        }

        [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll()
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(ErrorTokenInvalido);
            }

            if (!await _ticketService.IsStaffAsync(idUsuario.Value))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Acceso denegado. Se requieren permisos de Staff." });
            }

            var tickets = await _ticketService.GetAllTickets();
            return Ok(tickets);
        }

        [HttpGet("{id:int}/responses")]
        [ProducesResponseType(typeof(IEnumerable<TicketRespuestaDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetResponses(int id)
        {
            var respuestas = await _ticketService.GetTicketResponses(id);
            return Ok(respuestas);
        }

        [HttpPost("{id:int}/responses")]
        [ProducesResponseType(typeof(TicketRespuestaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendResponse(int id, [FromBody] CreateRespuestaDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(ErrorTokenInvalido);
            }

            var result = await _ticketService.AddResponse(id, idUsuario.Value, dto.Mensaje);

            if (result is not null)
            {
                await _hubContext.Clients.Group(id.ToString())
                    .SendAsync("ReceiveMessage", result);
            }

            return Ok(result);
        }

        [Authorize]
        [HttpPut("{id:int}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(ErrorTokenInvalido);
            }

            if (!await _ticketService.IsStaffAsync(idUsuario.Value))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Acceso denegado." });
            }

            var result = await _ticketService.UpdateTicketStatus(id, newStatus);
            if (!result) 
            {
                return NotFound(new { message = "Ticket no encontrado" });
            }

            await _hubContext.Clients.Group(id.ToString())
                .SendAsync("StatusUpdated", new { idTicket = id, nuevoEstado = newStatus });

            return Ok(new { message = "Estado actualizado con éxito" });
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CancelTicket(int id)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(ErrorTokenInvalido);
            }

            var result = await _ticketService.CancelTicket(id, idUsuario.Value);
            if (!result) 
            {
                return BadRequest(new { message = "No se pudo cancelar el ticket o no tienes permisos" });
            }

            return Ok(new { message = "Ticket cerrado/cancelado correctamente" });
        }
    }
}