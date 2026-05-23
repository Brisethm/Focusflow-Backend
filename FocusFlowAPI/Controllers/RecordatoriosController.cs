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
    public class RecordatoriosController : ControllerBase
    {
        private readonly IRecordatorioService _service;
        private const string InvalidUserIdMessage = "El token no contiene un identificador de usuario valido.";
        private const string RecordatorioNoEncontradoMessage = "Recordatorio no encontrado.";

        public RecordatoriosController(IRecordatorioService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RecordatorioDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRecordatorios()
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized(InvalidUserIdMessage);

            var recordatorios = await _service.ObtenerRecordatoriosAsync(idUsuario.Value);
            return Ok(recordatorios);
        }

        [HttpGet("{idRecordatorio:int}")]
        [ProducesResponseType(typeof(RecordatorioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRecordatorio(int idRecordatorio)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized(InvalidUserIdMessage);

            var recordatorio = await _service.ObtenerRecordatorioPorIdAsync(idUsuario.Value, idRecordatorio);
            return recordatorio == null ? NotFound(RecordatorioNoEncontradoMessage) : Ok(recordatorio);
        }

        [HttpPost]
        [ProducesResponseType(typeof(RecordatorioDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CrearRecordatorio([FromBody] RecordatorioDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized(InvalidUserIdMessage);

            var recordatorio = await _service.CrearRecordatorioAsync(idUsuario.Value, dto);
            return Ok(recordatorio);
        }

        [HttpPut("{idRecordatorio:int}")]
        [ProducesResponseType(typeof(RecordatorioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActualizarRecordatorio(int idRecordatorio, [FromBody] RecordatorioDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized(InvalidUserIdMessage);

            var recordatorio = await _service.ActualizarRecordatorioAsync(idUsuario.Value, idRecordatorio, dto);
            return recordatorio == null ? NotFound(RecordatorioNoEncontradoMessage) : Ok(recordatorio);
        }

        [HttpDelete("{idRecordatorio:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EliminarRecordatorio(int idRecordatorio)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
                return Unauthorized(InvalidUserIdMessage);

            var eliminado = await _service.EliminarRecordatorioAsync(idUsuario.Value, idRecordatorio);
            return eliminado ? NoContent() : NotFound(RecordatorioNoEncontradoMessage);
        }
    }
}
