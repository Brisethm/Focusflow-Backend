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
    public class RegistrosEmocionalesController : ControllerBase
    {
        private readonly IRegistroEmocionalService _service;

        private const string ErrorTokenInvalido = "El token no contiene un identificador de usuario válido.";

        public RegistrosEmocionalesController(IRegistroEmocionalService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RegistroEmocionalResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRegistros()
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(ErrorTokenInvalido);
            }

            var registros = await _service.ObtenerRegistrosAsync(idUsuario.Value);
            return Ok(registros);
        }

        [HttpGet("{idRegistro:int}")]
        [ProducesResponseType(typeof(RegistroEmocionalResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRegistro(int idRegistro)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(ErrorTokenInvalido);
            }

            var registro = await _service.ObtenerRegistroPorIdAsync(idUsuario.Value, idRegistro);
            return registro is null ? NotFound("Registro emocional no encontrado.") : Ok(registro);
        }

        [HttpPost]
        [ProducesResponseType(typeof(RegistroEmocionalResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CrearRegistro([FromBody] RegistroEmocionalDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(ErrorTokenInvalido);
            }

            var registro = await _service.CrearRegistroAsync(idUsuario.Value, dto);
            return Ok(registro);
        }

        [HttpPut("{idRegistro:int}")]
        [ProducesResponseType(typeof(RegistroEmocionalResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ActualizarRegistro(int idRegistro, [FromBody] RegistroEmocionalDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(ErrorTokenInvalido);
            }

            var registro = await _service.ActualizarRegistroAsync(idUsuario.Value, idRegistro, dto);
            return registro is null ? NotFound("Registro emocional no encontrado.") : Ok(registro);
        }

        [HttpDelete("{idRegistro:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> EliminarRegistro(int idRegistro)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(ErrorTokenInvalido);
            }

            var eliminado = await _service.EliminarRegistroAsync(idUsuario.Value, idRegistro);
            return eliminado ? NoContent() : NotFound("Registro emocional no encontrado.");
        }
    }
}