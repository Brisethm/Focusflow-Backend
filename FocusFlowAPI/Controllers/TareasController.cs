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
    public class TareasController : ControllerBase
    {
        private const string InvalidUserIdTokenMessage = "El token no contiene un identificador de usuario válido.";
        private readonly ITareaService _service;

        public TareasController(ITareaService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TareaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTareas()
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(InvalidUserIdTokenMessage);
            }

            var tareas = await _service.ObtenerTareasAsync(idUsuario.Value);
            return Ok(tareas);
        }

        [HttpGet("{idTarea:int}")]
        [ProducesResponseType(typeof(TareaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTarea(int idTarea)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(InvalidUserIdTokenMessage);
            }

            var tarea = await _service.ObtenerTareaPorIdAsync(idUsuario.Value, idTarea);
            return tarea is null ? NotFound("Tarea no encontrada.") : Ok(tarea);
        }

        [HttpPost]
        [ProducesResponseType(typeof(TareaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CrearTarea([FromBody] TareaRequestDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(InvalidUserIdTokenMessage);
            }

            var tarea = await _service.CrearTareaAsync(idUsuario.Value, dto);
            return Ok(tarea);
        }

        [HttpPut("{idTarea:int}")]
        [ProducesResponseType(typeof(TareaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ActualizarTarea(int idTarea, [FromBody] TareaRequestDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(InvalidUserIdTokenMessage);
            }

            var tarea = await _service.ActualizarTareaAsync(idUsuario.Value, idTarea, dto);
            return tarea is null ? NotFound("Tarea no encontrada.") : Ok(tarea);
        }

        [HttpDelete("{idTarea:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> EliminarTarea(int idTarea)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario is null)
            {
                return Unauthorized(InvalidUserIdTokenMessage);
            }

            var eliminada = await _service.EliminarTareaAsync(idUsuario.Value, idTarea);
            return eliminada ? NoContent() : NotFound("Tarea no encontrada.");
        }
    }
}