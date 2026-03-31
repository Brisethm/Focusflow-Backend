using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FocusFlowAPI.Services;
using FocusFlowAPI.DTOs;

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
            var claim = User.FindFirst("id_usuario");
            if (claim == null)
            {
                return Unauthorized("El token no contiene el claim 'id_usuario'.");
            }

            var idUsuario = Guid.Parse(claim.Value);
            var tareas = _service.ObtenerTareas(idUsuario);
            return Ok(tareas);
        }

        [HttpPost]
        [ProducesResponseType(typeof(TareaDto), StatusCodes.Status200OK)]
        public IActionResult CrearTarea([FromBody] TareaDto dto)
        {
            var claim = User.FindFirst("id_usuario");
            if (claim == null)
            {
                return Unauthorized("El token no contiene el claim 'id_usuario'.");
            }

            var idUsuario = Guid.Parse(claim.Value);
            var tarea = _service.CrearTarea(idUsuario, dto);
            return Ok(tarea);
        }

    }
}
