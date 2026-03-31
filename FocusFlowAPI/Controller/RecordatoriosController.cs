using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FocusFlowAPI.Services;
using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecordatoriosController : ControllerBase
    {
        private readonly RecordatorioService _service;

        public RecordatoriosController(RecordatorioService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RecordatorioDto>), StatusCodes.Status200OK)]
        public IActionResult GetRecordatorios()
        {
            var claim = User.FindFirst("id_usuario");
            if (claim == null)
            {
                return Unauthorized("El token no contiene el claim 'id_usuario'.");
            }

            var idUsuario = Guid.Parse(claim.Value);
            var recordatorios = _service.ObtenerRecordatorios(idUsuario);
            return Ok(recordatorios);
        }

        [HttpPost]
        [ProducesResponseType(typeof(RecordatorioDto), StatusCodes.Status200OK)]
        public IActionResult CrearRecordatorio([FromBody] RecordatorioDto dto)
        {
            var claim = User.FindFirst("id_usuario");
            if (claim == null)
            {
                return Unauthorized("El token no contiene el claim 'id_usuario'.");
            }

            var idUsuario = Guid.Parse(claim.Value);
            var recordatorio = _service.CrearRecordatorio(idUsuario, dto);
            return Ok(recordatorio);
        }
    }
}
