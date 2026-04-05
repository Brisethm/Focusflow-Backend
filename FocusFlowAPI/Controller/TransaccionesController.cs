using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FocusFlowAPI.Services;
using FocusFlowAPI.DTOs;
using FocusFlowAPI.Extensions;

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
        public IActionResult GetTransacciones()
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
            {
                return Unauthorized("El token no contiene un identificador de usuario válido.");
            }

            var transacciones = _service.ObtenerTransacciones(idUsuario.Value);
            return Ok(transacciones);
        }

        [HttpPost]
        [ProducesResponseType(typeof(TransaccionDto), StatusCodes.Status200OK)]
        public IActionResult CrearTransaccion([FromBody] TransaccionDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
            {
                return Unauthorized("El token no contiene un identificador de usuario válido.");
            }

            var transaccion = _service.CrearTransaccion(idUsuario.Value, dto);
            return Ok(transaccion);
        }

    }
}
