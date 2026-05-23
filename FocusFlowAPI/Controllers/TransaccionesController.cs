using FocusFlowAPI.DTOs;
using FocusFlowAPI.Extensions;
using FocusFlowAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FocusFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransaccionesController : ControllerBase
    {
        private readonly ITransaccionService _service;

        private const string ErrorTokenInvalido = "El token no contiene un identificador de usuario válido.";

        public TransaccionesController(ITransaccionService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TransaccionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTransacciones()
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(ErrorTokenInvalido);
            }

            var transacciones = await _service.ObtenerTransaccionesAsync(idUsuario.Value);
            return Ok(transacciones);
        }

        [HttpGet("{idTransaccion:int}")]
        [ProducesResponseType(typeof(TransaccionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTransaccion(int idTransaccion)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(ErrorTokenInvalido);
            }

            var transaccion = await _service.ObtenerTransaccionPorIdAsync(idUsuario.Value, idTransaccion);
            return transaccion is null ? NotFound("Transacción no encontrada.") : Ok(transaccion);
        }

        [HttpPost]
        [ProducesResponseType(typeof(TransaccionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CrearTransaccion([FromBody] TransaccionDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(ErrorTokenInvalido);
            }

            var transaccion = await _service.CrearTransaccionAsync(idUsuario.Value, dto);
            return Ok(transaccion);
        }

        [HttpPut("{idTransaccion:int}")]
        [ProducesResponseType(typeof(TransaccionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ActualizarTransaccion(int idTransaccion, [FromBody] TransaccionDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(ErrorTokenInvalido);
            }

            var transaccion = await _service.ActualizarTransaccionAsync(idUsuario.Value, idTransaccion, dto);
            return transaccion is null ? NotFound("Transacción no encontrada.") : Ok(transaccion);
        }

        [HttpDelete("{idTransaccion:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> EliminarTransaccion(int idTransaccion)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(ErrorTokenInvalido);
            }

            var eliminada = await _service.EliminarTransaccionAsync(idUsuario.Value, idTransaccion);
            return eliminada ? NoContent() : NotFound("Transacción no encontrada.");
        }
    }
}