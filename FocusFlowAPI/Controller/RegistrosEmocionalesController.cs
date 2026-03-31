using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FocusFlowAPI.Services;
using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RegistrosEmocionalesController : ControllerBase
    {
        private readonly RegistroEmocionalService _service;

        public RegistrosEmocionalesController(RegistroEmocionalService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RegistroEmocionalDto>), StatusCodes.Status200OK)]
        public IActionResult GetRegistros()
        {
            var claim = User.FindFirst("id_usuario");
            if (claim == null)
            {
                return Unauthorized("El token no contiene el claim 'id_usuario'.");
            }

            var idUsuario = Guid.Parse(claim.Value);
            var registros = _service.ObtenerRegistros(idUsuario);
            return Ok(registros);
        }

        [HttpPost]
        [ProducesResponseType(typeof(RegistroEmocionalDto), StatusCodes.Status200OK)]
        public IActionResult CrearRegistro([FromBody] RegistroEmocionalDto dto)
        {
            var claim = User.FindFirst("id_usuario");
            if (claim == null)
            {
                return Unauthorized("El token no contiene el claim 'id_usuario'.");
            }

            var idUsuario = Guid.Parse(claim.Value);
            var registro = _service.CrearRegistro(idUsuario, dto);
            return Ok(registro);
        }

    }
}
