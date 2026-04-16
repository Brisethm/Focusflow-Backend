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
        public async Task<IActionResult> GetCuestionarios()
        {
            var idUsuario = User.GetAuthenticatedUserId();

            if (idUsuario == null)
            {
                return Unauthorized("El token no contiene un identificador de usuario valido.");
            }

            var cuestionarios = await _service.ObtenerCuestionariosAsync(idUsuario.Value);
            return Ok(cuestionarios);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CuestionarioDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CrearCuestionario([FromBody] CuestionarioDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();

            if (idUsuario == null)
            {
                return Unauthorized("El token no contiene un identificador de usuario valido.");
            }

            var cuestionario = await _service.CrearCuestionarioAsync(idUsuario.Value, dto);
            return Ok(cuestionario);
        }
    }
}