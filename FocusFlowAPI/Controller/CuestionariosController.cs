using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FocusFlowAPI.Services;
using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CuestionariosController : ControllerBase
    {
        private readonly CuestionarioService _service;

        public CuestionariosController(CuestionarioService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CuestionarioDto>), StatusCodes.Status200OK)]
        public IActionResult GetCuestionarios()
        {
            var claim = User.FindFirst("sub");
            if (claim == null)
            {
                return Unauthorized("El token no contiene el claim 'sub'.");
            }

            var idUsuario = Guid.Parse(claim.Value);
            var cuestionarios = _service.ObtenerCuestionarios(idUsuario);
            return Ok(cuestionarios);
        }


        [HttpPost]
        [ProducesResponseType(typeof(CuestionarioDto), StatusCodes.Status200OK)]
        public IActionResult CrearCuestionario([FromBody] CuestionarioDto dto)
        {
            var claim = User.FindFirst("sub");
            if (claim == null)
            {
                return Unauthorized("El token no contiene el claim 'sub'.");
            }

            var idUsuario = Guid.Parse(claim.Value);
            var cuestionario = _service.CrearCuestionario(idUsuario, dto);
            return Ok(cuestionario);
        }
    }
}
