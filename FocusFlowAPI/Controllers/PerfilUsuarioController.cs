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
    public class PerfilUsuarioController : ControllerBase
    {
        private readonly IPerfilUsuarioService _service;
        private const string InvalidUserIdTokenMessage = "El token no contiene un identificador de usuario válido.";

        public PerfilUsuarioController(IPerfilUsuarioService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PerfilUsuarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPerfil()
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(InvalidUserIdTokenMessage);
            }

            var perfil = await _service.ObtenerPerfilAsync(idUsuario.Value);
            return perfil is null ? NotFound("Perfil no encontrado.") : Ok(perfil);
        }

        [HttpPost]
        [ProducesResponseType(typeof(PerfilUsuarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CrearPerfil([FromBody] PerfilUsuarioDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(InvalidUserIdTokenMessage);
            }

            dto.Rol = "user";

            var perfil = await _service.CrearPerfilAsync(idUsuario.Value, dto);
            return Ok(perfil);
        }

        [HttpPut]
        [ProducesResponseType(typeof(PerfilUsuarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ActualizarPerfil([FromBody] PerfilUsuarioDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(InvalidUserIdTokenMessage);
            }

            dto.Rol = null;

            var perfil = await _service.ActualizarPerfilAsync(idUsuario.Value, dto);
            return perfil is null ? NotFound("Perfil no encontrado.") : Ok(perfil);
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> EliminarPerfil()
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(InvalidUserIdTokenMessage);
            }

            var eliminado = await _service.EliminarPerfilAsync(idUsuario.Value);
            return eliminado ? NoContent() : NotFound("Perfil no encontrado.");
        }
    }
}