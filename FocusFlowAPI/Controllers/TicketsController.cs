using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FocusFlowAPI.Services;
using FocusFlowAPI.DTOs;
using FocusFlowAPI.Extensions;

namespace FocusFlowAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly TicketService _ticketService;

        public TicketsController(TicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet("my-tickets")]
        [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyTickets()
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario válido.");

            return Ok(await _ticketService.GetUserTickets(idUsuario.Value));
        }

        [HttpPost]
        [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario válido.");

            var result = await _ticketService.CreateTicket(idUsuario.Value, dto);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario válido.");
            if (!await _ticketService.IsStaffAsync(idUsuario.Value))
            {
                return StatusCode(403, new { message = "Acceso denegado. Se requieren permisos de Staff." });
            }

            return Ok(await _ticketService.GetAllTickets());
        }
        [HttpGet("{id:int}/responses")]
        public async Task<IActionResult> GetResponses(int id)
        {
            return Ok(await _ticketService.GetTicketResponses(id));
        }

        [HttpPost("{id:int}/responses")]
        public async Task<IActionResult> SendResponse(int id, [FromBody] CreateRespuestaDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario válido.");

            var result = await _ticketService.AddResponse(id, idUsuario.Value, dto.Mensaje);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario válido.");

            if (!await _ticketService.IsStaffAsync(idUsuario.Value))
            {
                return StatusCode(403, new { message = "Acceso denegado. Se requieren permisos de Staff." });
            }

            var result = await _ticketService.UpdateTicketStatus(id, newStatus);
            if (!result) return NotFound(new { message = "Ticket no encontrado" });

            return Ok(new { message = "Estado actualizado con éxito" });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> CancelTicket(int id)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario válido.");

            var result = await _ticketService.CancelTicket(id, idUsuario.Value);

            if (!result) return BadRequest(new { message = "No se pudo cancelar el ticket o no tienes permisos" });

            return Ok(new { message = "Ticket cerrado/cancelado correctamente" });
        }
    }
}