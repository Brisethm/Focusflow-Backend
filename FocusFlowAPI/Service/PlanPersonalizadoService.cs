using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FocusFlowAPI.Services
{
    public class PlanPersonalizadoService
    {
        private readonly UsuarioContext _context;
        private readonly ILogger<PlanPersonalizadoService> _logger;

        public PlanPersonalizadoService(UsuarioContext context, ILogger<PlanPersonalizadoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<PlanPersonalizadoDto>> ObtenerPlanesAsync(Guid idUsuario)
        {
            return await _context.PlanesPersonalizados
                .AsNoTracking()
                .Where(p => p.IdUsuario == idUsuario)
                .OrderByDescending(p => p.FechaCreacion)
                .Select(p => MapToDto(p))
                .ToListAsync();
        }

        public async Task<PlanPersonalizadoDto?> ObtenerPlanPorIdAsync(Guid idUsuario, int idPlan)
        {
            var plan = await _context.PlanesPersonalizados
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdUsuario == idUsuario && p.IdPlan == idPlan);

            return plan == null ? null : MapToDto(plan);
        }

        public async Task<PlanPersonalizadoDto> CrearPlanAsync(Guid idUsuario, CreatePlanDto dto)
        {
            _logger.LogInformation("[PlanPersonalizado] Iniciando creación. Usuario={Usuario}, IdCuestionario={IdCuestionario}",
                idUsuario, dto.IdCuestionario);

            var plan = new PlanPersonalizado
            {
                IdUsuario = idUsuario,
                IdCuestionario = dto.IdCuestionario,
                HoraDescanso = dto.HoraDescanso!.Value,
                EnfoqueDiario = dto.EnfoqueDiario!.Value,
                PausasDiarias = dto.PausasDiarias!.Value
            };

            _context.PlanesPersonalizados.Add(plan);

            _logger.LogInformation("[PlanPersonalizado] Guardando plan en base de datos...");
            await _context.SaveChangesAsync();

            _logger.LogInformation("[PlanPersonalizado] Plan guardado correctamente. IdPlan={IdPlan}", plan.IdPlan);
            return MapToDto(plan);
        }

        public async Task<PlanPersonalizadoDto?> ActualizarPlanAsync(Guid idUsuario, int idPlan, CreatePlanDto dto)
        {
            _logger.LogInformation("[PlanPersonalizado] Actualizando plan {IdPlan}", idPlan);

            var filas = await _context.PlanesPersonalizados
                .Where(p => p.IdUsuario == idUsuario && p.IdPlan == idPlan)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.IdCuestionario, dto.IdCuestionario)
                    .SetProperty(p => p.HoraDescanso, dto.HoraDescanso!.Value)
                    .SetProperty(p => p.EnfoqueDiario, dto.EnfoqueDiario!.Value)
                    .SetProperty(p => p.PausasDiarias, dto.PausasDiarias!.Value)
                );

            if (filas == 0)
                return null;

            _logger.LogInformation("[PlanPersonalizado] Plan {IdPlan} actualizado correctamente.", idPlan);

            return new PlanPersonalizadoDto
            {
                IdPlan = idPlan,
                IdUsuario = idUsuario,
                IdCuestionario = dto.IdCuestionario,
                HoraDescanso = dto.HoraDescanso!.Value,
                EnfoqueDiario = dto.EnfoqueDiario!.Value,
                PausasDiarias = dto.PausasDiarias!.Value
            };
        }

        public async Task<bool> EliminarPlanAsync(Guid idUsuario, int idPlan)
        {
            var filas = await _context.PlanesPersonalizados
                .Where(p => p.IdUsuario == idUsuario && p.IdPlan == idPlan)
                .ExecuteDeleteAsync();

            return filas > 0;
        }

        private static PlanPersonalizadoDto MapToDto(PlanPersonalizado plan)
        {
            return new PlanPersonalizadoDto
            {
                IdPlan = plan.IdPlan,
                IdUsuario = plan.IdUsuario,
                IdCuestionario = plan.IdCuestionario,
                HoraDescanso = plan.HoraDescanso,
                EnfoqueDiario = plan.EnfoqueDiario,
                PausasDiarias = plan.PausasDiarias,
                FechaCreacion = plan.FechaCreacion
            };
        }
    }
}