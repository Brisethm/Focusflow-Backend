using FocusFlowAPI.DTOs;
using FocusFlowAPI.Extensions;
using FocusFlowAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FocusFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Todo requiere estar logueado
    public class PerfilUsuarioController : ControllerBase
    {
        private readonly PerfilUsuarioService _service;

        public PerfilUsuarioController(PerfilUsuarioService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PerfilUsuarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPerfil()
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var perfil = await _service.ObtenerPerfilAsync(idUsuario.Value);
            return perfil == null ? NotFound("Perfil no encontrado.") : Ok(perfil);
        }

        [HttpPost]
        [ProducesResponseType(typeof(PerfilUsuarioDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CrearPerfil([FromBody] PerfilUsuarioDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");
            dto.Rol = "user"; 

            var perfil = await _service.CrearPerfilAsync(idUsuario.Value, dto);
            return Ok(perfil);
        }

        [HttpPut]
        [ProducesResponseType(typeof(PerfilUsuarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActualizarPerfil([FromBody] PerfilUsuarioDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            // 🛡️ PROTECCIÓN: Evitamos que el usuario cambie su propio rol en una actualización.
            // Al ponerlo en null, tu PerfilUsuarioService debe saber que NO debe actualizar esa columna.
            dto.Rol = null; 

            var perfil = await _service.ActualizarPerfilAsync(idUsuario.Value, dto);
            return perfil == null ? NotFound("Perfil no encontrado.") : Ok(perfil);
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EliminarPerfil()
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var eliminado = await _service.EliminarPerfilAsync(idUsuario.Value);
            return eliminado ? NoContent() : NotFound("Perfil no encontrado.");
        }
    }
}