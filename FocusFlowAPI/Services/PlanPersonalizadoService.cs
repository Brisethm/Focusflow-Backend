using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FocusFlowAPI.Services
{
    public class PlanPersonalizadoService : IPlanPersonalizadoService
    {
        private readonly UsuarioContext _context;
        private readonly ILogger<PlanPersonalizadoService> _logger;

        public PlanPersonalizadoService(UsuarioContext context, ILogger<PlanPersonalizadoService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<PlanPersonalizadoDto>> ObtenerPlanesAsync(Guid idUsuario)
        {
            return await _context.PlanesPersonalizados
                .AsNoTracking()
                .Where(p => p.IdUsuario == idUsuario)
                .OrderByDescending(p => p.FechaCreacion)
                .Select(p => new PlanPersonalizadoDto
                {
                    IdPlan = p.IdPlan,
                    IdUsuario = p.IdUsuario,
                    IdCuestionario = p.IdCuestionario,
                    HoraDescanso = p.HoraDescanso,
                    EnfoqueDiario = p.EnfoqueDiario,
                    PausasDiarias = p.PausasDiarias,
                    FechaCreacion = p.FechaCreacion
                })
                .ToListAsync();
        }

        public async Task<PlanPersonalizadoDto?> ObtenerPlanPorIdAsync(Guid idUsuario, int idPlan)
        {
            var plan = await _context.PlanesPersonalizados
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdUsuario == idUsuario && p.IdPlan == idPlan);

            return plan is null ? null : MapToDto(plan);
        }

        public async Task<PlanPersonalizadoDto> CrearPlanAsync(Guid idUsuario, CreatePlanDto dto)
        {
            _logger.LogInformation("[PlanPersonalizado] Iniciando creación. Usuario={Usuario}, IdCuestionario={IdCuestionario}",
                idUsuario, dto.IdCuestionario);

            var plan = new PlanPersonalizado
            {
                IdUsuario = idUsuario,
                IdCuestionario = dto.IdCuestionario,
                HoraDescanso = dto.HoraDescanso ?? TimeOnly.MinValue,
                EnfoqueDiario = dto.EnfoqueDiario ?? 0,
                PausasDiarias = dto.PausasDiarias ?? 0,
                FechaCreacion = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };

            _context.PlanesPersonalizados.Add(plan);

            _logger.LogDebug("[PlanPersonalizado] Guardando plan en base de datos...");
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
                    .SetProperty(p => p.HoraDescanso, dto.HoraDescanso ?? TimeOnly.MinValue)
                    .SetProperty(p => p.EnfoqueDiario, dto.EnfoqueDiario ?? 0)
                    .SetProperty(p => p.PausasDiarias, dto.PausasDiarias ?? 0)
                );

            if (filas is 0)
            {
                return null;
            }

            _logger.LogInformation("[PlanPersonalizado] Plan {IdPlan} updated successfully.", idPlan);

            return new PlanPersonalizadoDto
            {
                IdPlan = idPlan,
                IdUsuario = idUsuario,
                IdCuestionario = dto.IdCuestionario,
                HoraDescanso = dto.HoraDescanso ?? TimeOnly.MinValue,
                EnfoqueDiario = dto.EnfoqueDiario ?? 0,
                PausasDiarias = dto.PausasDiarias ?? 0
            };
        }

        public async Task<bool> EliminarPlanAsync(Guid idUsuario, int idPlan)
        {
            var filas = await _context.PlanesPersonalizados
                .Where(p => p.IdUsuario == idUsuario && p.IdPlan == idPlan)
                .ExecuteDeleteAsync();

            return filas > 0;
        }

        private static PlanPersonalizadoDto MapToDto(PlanPersonalizado plan) =>
            new()
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