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
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
            {
                return Unauthorized("El token no contiene un identificador de usuario válido.");
            }

            var cuestionarios = _service.ObtenerCuestionarios(idUsuario.Value);
            return Ok(cuestionarios);
        }


        [HttpPost]
        [ProducesResponseType(typeof(CuestionarioDto), StatusCodes.Status200OK)]
        public IActionResult CrearCuestionario([FromBody] CuestionarioDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
            {
                return Unauthorized("El token no contiene un identificador de usuario válido.");
            }

            var cuestionario = _service.CrearCuestionario(idUsuario.Value, dto);
            return Ok(cuestionario);
        }
    }
}
