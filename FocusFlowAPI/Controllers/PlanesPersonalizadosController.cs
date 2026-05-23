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
    public class PlanesPersonalizadosController : ControllerBase
    {
        private readonly IPlanPersonalizadoService _service;
        private const string InvalidUserIdError = "El token no contiene un identificador de usuario válido.";

        public PlanesPersonalizadosController(IPlanPersonalizadoService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PlanPersonalizadoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPlanes()
        {
            var idUsuario = ObtenerIdUsuarioDesdeToken();
            if (idUsuario is null)
            {
                return Unauthorized(InvalidUserIdError);
            }

            var planes = await _service.ObtenerPlanesAsync(idUsuario.Value);
            return Ok(planes);
        }

        [HttpGet("{idPlan:int}")]
        [ProducesResponseType(typeof(PlanPersonalizadoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPlan(int idPlan)
        {
            var idUsuario = ObtenerIdUsuarioDesdeToken();
            if (idUsuario is null)
            {
                return Unauthorized(InvalidUserIdError);
            }

            var plan = await _service.ObtenerPlanPorIdAsync(idUsuario.Value, idPlan);
            return plan is null ? NotFound("Plan personalizado no encontrado.") : Ok(plan);
        }

        [HttpPost]
        [ProducesResponseType(typeof(PlanPersonalizadoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CrearPlan([FromBody] CreatePlanDto dto)
        {
            var idUsuario = ObtenerIdUsuarioDesdeToken();
            if (idUsuario is null)
            {
                return Unauthorized(InvalidUserIdError);
            }

            try
            {
                var plan = await _service.CrearPlanAsync(idUsuario.Value, dto);
                return CreatedAtAction(nameof(GetPlan), new { idPlan = plan.IdPlan }, plan);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status409Conflict, ex.Message);
            }
        }

        [HttpPut("{idPlan:int}")]
        [ProducesResponseType(typeof(PlanPersonalizadoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ActualizarPlan(int idPlan, [FromBody] CreatePlanDto dto)
        {
            var idUsuario = ObtenerIdUsuarioDesdeToken();
            if (idUsuario is null)
            {
                return Unauthorized(InvalidUserIdError);
            }

            try
            {
                var plan = await _service.ActualizarPlanAsync(idUsuario.Value, idPlan, dto);
                return plan is null ? NotFound("Plan personalizado no encontrado.") : Ok(plan);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status409Conflict, ex.Message);
            }
        }

        [HttpDelete("{idPlan:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> EliminarPlan(int idPlan)
        {
            var idUsuario = ObtenerIdUsuarioDesdeToken();
            if (idUsuario is null)
            {
                return Unauthorized(InvalidUserIdError);
            }

            var eliminado = await _service.EliminarPlanAsync(idUsuario.Value, idPlan);
            return eliminado ? NoContent() : NotFound("Plan personalizado no encontrado.");
        }

        private Guid? ObtenerIdUsuarioDesdeToken() => User.GetAuthenticatedUserId();
    }
}