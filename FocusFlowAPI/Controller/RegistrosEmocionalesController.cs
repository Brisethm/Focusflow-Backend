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
            {
                return Unauthorized("El token no contiene un identificador de usuario valido.");
            }

            var registros = await _service.ObtenerRegistrosAsync(idUsuario.Value);
            return Ok(registros);
        }

        [HttpPost]
        [ProducesResponseType(typeof(RegistroEmocionalResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CrearRegistro([FromBody] RegistroEmocionalDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
            {
                return Unauthorized("El token no contiene un identificador de usuario valido.");
            }

            var registro = await _service.CrearRegistroAsync(idUsuario.Value, dto);
            return Ok(registro);
        }
    }
}
