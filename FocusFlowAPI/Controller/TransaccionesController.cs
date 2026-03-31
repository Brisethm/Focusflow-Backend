using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FocusFlowAPI.Services;
using FocusFlowAPI.DTOs;

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
            var claim = User.FindFirst("id_usuario");
            if (claim == null)
            {
                return Unauthorized("El token no contiene el claim 'id_usuario'.");
            }

            var idUsuario = Guid.Parse(claim.Value);
            var transacciones = _service.ObtenerTransacciones(idUsuario);
            return Ok(transacciones);
        }

        [HttpPost]
        [ProducesResponseType(typeof(TransaccionDto), StatusCodes.Status200OK)]
        public IActionResult CrearTransaccion([FromBody] TransaccionDto dto)
        {
            var claim = User.FindFirst("id_usuario");
            if (claim == null)
            {
                return Unauthorized("El token no contiene el claim 'id_usuario'.");
            }

            var idUsuario = Guid.Parse(claim.Value);
            var transaccion = _service.CrearTransaccion(idUsuario, dto);
            return Ok(transaccion);
        }

    }
}
