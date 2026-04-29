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
        private readonly RegistroEmocionalService _service;

        public RegistrosEmocionalesController(RegistroEmocionalService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RegistroEmocionalResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRegistros()
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var registros = await _service.ObtenerRegistrosAsync(idUsuario.Value);
            return Ok(registros);
        }

        [HttpGet("{idRegistro:int}")]
        [ProducesResponseType(typeof(RegistroEmocionalResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRegistro(int idRegistro)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var registro = await _service.ObtenerRegistroPorIdAsync(idUsuario.Value, idRegistro);
            return registro == null ? NotFound("Registro emocional no encontrado.") : Ok(registro);
        }

        [HttpPost]
        [ProducesResponseType(typeof(RegistroEmocionalResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CrearRegistro([FromBody] RegistroEmocionalDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var registro = await _service.CrearRegistroAsync(idUsuario.Value, dto);
            return Ok(registro);
        }

        [HttpPut("{idRegistro:int}")]
        [ProducesResponseType(typeof(RegistroEmocionalResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActualizarRegistro(int idRegistro, [FromBody] RegistroEmocionalDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var registro = await _service.ActualizarRegistroAsync(idUsuario.Value, idRegistro, dto);
            return registro == null ? NotFound("Registro emocional no encontrado.") : Ok(registro);
        }

        [HttpDelete("{idRegistro:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EliminarRegistro(int idRegistro)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var eliminado = await _service.EliminarRegistroAsync(idUsuario.Value, idRegistro);
            return eliminado ? NoContent() : NotFound("Registro emocional no encontrado.");
        }
    }
}
