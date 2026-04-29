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
        private readonly TransaccionService _service;

        public TransaccionesController(TransaccionService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TransaccionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTransacciones()
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var transacciones = await _service.ObtenerTransaccionesAsync(idUsuario.Value);
            return Ok(transacciones);
        }

        [HttpGet("{idTransaccion:int}")]
        [ProducesResponseType(typeof(TransaccionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTransaccion(int idTransaccion)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var transaccion = await _service.ObtenerTransaccionPorIdAsync(idUsuario.Value, idTransaccion);
            return transaccion == null ? NotFound("Transaccion no encontrada.") : Ok(transaccion);
        }

        [HttpPost]
        [ProducesResponseType(typeof(TransaccionDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CrearTransaccion([FromBody] TransaccionDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var transaccion = await _service.CrearTransaccionAsync(idUsuario.Value, dto);
            return Ok(transaccion);
        }

        [HttpPut("{idTransaccion:int}")]
        [ProducesResponseType(typeof(TransaccionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActualizarTransaccion(int idTransaccion, [FromBody] TransaccionDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var transaccion = await _service.ActualizarTransaccionAsync(idUsuario.Value, idTransaccion, dto);
            return transaccion == null ? NotFound("Transaccion no encontrada.") : Ok(transaccion);
        }

        [HttpDelete("{idTransaccion:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EliminarTransaccion(int idTransaccion)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var eliminada = await _service.EliminarTransaccionAsync(idUsuario.Value, idTransaccion);
            return eliminada ? NoContent() : NotFound("Transaccion no encontrada.");
        }
    }
}
