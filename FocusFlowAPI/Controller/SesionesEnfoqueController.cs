using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FocusFlowAPI.Services;
using FocusFlowAPI.DTOs;

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
            var claim = User.FindFirst("id_usuario");
            if (claim == null)
            {
                return Unauthorized("El token no contiene el claim 'id_usuario'.");
            }

            var idUsuario = Guid.Parse(claim.Value);
            var sesiones = _service.ObtenerSesiones(idUsuario);
            return Ok(sesiones);
        }

        [HttpPost]
        [ProducesResponseType(typeof(SesionEnfoqueDto), StatusCodes.Status200OK)]
        public IActionResult CrearSesion([FromBody] SesionEnfoqueDto dto)
        {
            var claim = User.FindFirst("id_usuario");
            if (claim == null)
            {
                return Unauthorized("El token no contiene el claim 'id_usuario'.");
            }

            var idUsuario = Guid.Parse(claim.Value);
            var sesion = _service.CrearSesion(idUsuario, dto);
            return Ok(sesion);
        }

    }
}
