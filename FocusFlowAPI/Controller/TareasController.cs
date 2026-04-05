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
    public class TareasController : ControllerBase
    {
        private readonly TareaService _service;

        public TareasController(TareaService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TareaDto>), StatusCodes.Status200OK)]
        public IActionResult GetTareas()
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
            {
                return Unauthorized("El token no contiene un identificador de usuario válido.");
            }

            var tareas = _service.ObtenerTareas(idUsuario.Value);
            return Ok(tareas);
        }

        [HttpPost]
        [ProducesResponseType(typeof(TareaDto), StatusCodes.Status200OK)]
        public IActionResult CrearTarea([FromBody] TareaDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
            {
                return Unauthorized("El token no contiene un identificador de usuario válido.");
            }

            var tarea = _service.CrearTarea(idUsuario.Value, dto);
            return Ok(tarea);
        }

    }
}
