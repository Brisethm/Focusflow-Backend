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
        private readonly SesionEnfoqueService _service;

        public SesionesEnfoqueController(SesionEnfoqueService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SesionEnfoqueDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSesiones()
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var sesiones = await _service.ObtenerSesionesAsync(idUsuario.Value);
            return Ok(sesiones);
        }

        [HttpGet("{idSesion:int}")]
        [ProducesResponseType(typeof(SesionEnfoqueDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSesion(int idSesion)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var sesion = await _service.ObtenerSesionPorIdAsync(idUsuario.Value, idSesion);
            return sesion == null ? NotFound("Sesion no encontrada.") : Ok(sesion);
        }

        [HttpPost]
        [ProducesResponseType(typeof(SesionEnfoqueDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CrearSesion([FromBody] SesionEnfoqueDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var sesion = await _service.CrearSesionAsync(idUsuario.Value, dto);
            return Ok(sesion);
        }

        [HttpPut("{idSesion:int}")]
        [ProducesResponseType(typeof(SesionEnfoqueDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActualizarSesion(int idSesion, [FromBody] SesionEnfoqueDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var sesion = await _service.ActualizarSesionAsync(idUsuario.Value, idSesion, dto);
            return sesion == null ? NotFound("Sesion no encontrada.") : Ok(sesion);
        }

        [HttpDelete("{idSesion:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EliminarSesion(int idSesion)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var eliminada = await _service.EliminarSesionAsync(idUsuario.Value, idSesion);
            return eliminada ? NoContent() : NotFound("Sesion no encontrada.");
        }
    }
}
