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
        private readonly TareaService _service;

        public TareasController(TareaService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TareaDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTareas()
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var tareas = await _service.ObtenerTareasAsync(idUsuario.Value);
            return Ok(tareas);
        }

        [HttpGet("{idTarea:int}")]
        [ProducesResponseType(typeof(TareaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTarea(int idTarea)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var tarea = await _service.ObtenerTareaPorIdAsync(idUsuario.Value, idTarea);
            return tarea == null ? NotFound("Tarea no encontrada.") : Ok(tarea);
        }

        [HttpPost]
        [ProducesResponseType(typeof(TareaDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CrearTarea([FromBody] TareaDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var tarea = await _service.CrearTareaAsync(idUsuario.Value, dto);
            return Ok(tarea);
        }

        [HttpPut("{idTarea:int}")]
        [ProducesResponseType(typeof(TareaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActualizarTarea(int idTarea, [FromBody] TareaDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var tarea = await _service.ActualizarTareaAsync(idUsuario.Value, idTarea, dto);
            return tarea == null ? NotFound("Tarea no encontrada.") : Ok(tarea);
        }

        [HttpDelete("{idTarea:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EliminarTarea(int idTarea)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized("El token no contiene un identificador de usuario valido.");

            var eliminada = await _service.EliminarTareaAsync(idUsuario.Value, idTarea);
            return eliminada ? NoContent() : NotFound("Tarea no encontrada.");
        }
    }
}
