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
    public class SesionesEnfoqueController : ControllerBase
    {
        private readonly SesionEnfoqueService _service;

        public SesionesEnfoqueController(SesionEnfoqueService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SesionEnfoqueDto>), StatusCodes.Status200OK)]
        public IActionResult GetSesiones()
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
            {
                return Unauthorized("El token no contiene un identificador de usuario válido.");
            }

            var sesiones = _service.ObtenerSesiones(idUsuario.Value);
            return Ok(sesiones);
        }

        [HttpPost]
        [ProducesResponseType(typeof(SesionEnfoqueDto), StatusCodes.Status200OK)]
        public IActionResult CrearSesion([FromBody] SesionEnfoqueDto dto)
        {
            var idUsuario = User.GetAuthenticatedUserId();
            if (idUsuario == null)
            {
                return Unauthorized("El token no contiene un identificador de usuario válido.");
            }

            var sesion = _service.CrearSesion(idUsuario.Value, dto);
            return Ok(sesion);
        }

    }
}
