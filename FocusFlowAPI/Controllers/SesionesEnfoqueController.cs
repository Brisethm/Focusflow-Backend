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
    public class SesionesEnfoqueController : ControllerBase
    {
        private const string InvalidUserIdTokenMessage = "El token no contiene un identificador de usuario válido.";
        private const string SessionNotFoundMessage = "Sesión no encontrada.";
        private readonly ISesionEnfoqueService _service;
        public SesionesEnfoqueController(ISesionEnfoqueService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SesionEnfoqueDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSesiones()
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (!idUsuario.HasValue)
            {
                return Unauthorized(InvalidUserIdTokenMessage);
            }

            var sesiones = await _service.ObtenerSesionesAsync(idUsuario.Value);
            return Ok(sesiones);
        }

        [HttpGet("{idSesion:int}")]
        [ProducesResponseType(typeof(SesionEnfoqueDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSesion(int idSesion)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (!idUsuario.HasValue)
            {
                return Unauthorized(InvalidUserIdTokenMessage);
            }

            var sesion = await _service.ObtenerSesionPorIdAsync(idUsuario.Value, idSesion);
            return sesion is null ? NotFound(SessionNotFoundMessage) : Ok(sesion);
        }

        [HttpPost]
        [ProducesResponseType(typeof(SesionEnfoqueDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CrearSesion([FromBody] SesionEnfoqueDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (!idUsuario.HasValue)
            {
                return Unauthorized(InvalidUserIdTokenMessage);
            }

            var sesion = await _service.CrearSesionAsync(idUsuario.Value, dto);
            return Ok(sesion);
        }

        [HttpPut("{idSesion:int}")]
        [ProducesResponseType(typeof(SesionEnfoqueDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ActualizarSesion(int idSesion, [FromBody] SesionEnfoqueDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (!idUsuario.HasValue)
            {
                return Unauthorized(InvalidUserIdTokenMessage);
            }

            var sesion = await _service.ActualizarSesionAsync(idUsuario.Value, idSesion, dto);
            return sesion is null ? NotFound(SessionNotFoundMessage) : Ok(sesion);
        }

        [HttpDelete("{idSesion:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> EliminarSesion(int idSesion)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (!idUsuario.HasValue)
            {
                return Unauthorized(InvalidUserIdTokenMessage);
            }

            var eliminada = await _service.EliminarSesionAsync(idUsuario.Value, idSesion);
            return eliminada ? NoContent() : NotFound(SessionNotFoundMessage);
        }
    }
}