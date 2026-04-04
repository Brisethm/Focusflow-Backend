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
            var claim = User.FindFirst("sub");
            if (claim == null)
            {
                return Unauthorized("El token no contiene el claim 'sub'.");
            }

            var sub = Guid.Parse(claim.Value);
            var registros = _service.ObtenerRegistros(sub);
            return Ok(registros);
        }

        [HttpPost]
        [ProducesResponseType(typeof(RegistroEmocionalDto), StatusCodes.Status200OK)]
        public IActionResult CrearRegistro([FromBody] RegistroEmocionalDto dto)
        {
            var claim = User.FindFirst("sub");
            if (claim == null)
            {
                return Unauthorized("El token no contiene el claim 'sub'.");
            }

            var sub = Guid.Parse(claim.Value);
            var registro = _service.CrearRegistro(sub, dto);
            return Ok(registro);
        }

    }
}
