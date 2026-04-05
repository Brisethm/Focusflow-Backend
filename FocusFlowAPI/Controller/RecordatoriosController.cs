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
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
            {
                return Unauthorized("El token no contiene un identificador de usuario válido.");
            }

            var recordatorios = _service.ObtenerRecordatorios(idUsuario.Value);
            return Ok(recordatorios);
        }

        [HttpPost]
        [ProducesResponseType(typeof(RecordatorioDto), StatusCodes.Status200OK)]
        public IActionResult CrearRecordatorio([FromBody] RecordatorioDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
            {
                return Unauthorized("El token no contiene un identificador de usuario válido.");
            }

            var recordatorio = _service.CrearRecordatorio(idUsuario.Value, dto);
            return Ok(recordatorio);
        }
    }
}
