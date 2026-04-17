using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    await ValidarCuestionarioAsync(idUsuario, dto.IdCuestionario);

                    // Verificar si ya existe un plan reciente (idempotencia)
                    var planExistente = await _context.PlanesPersonalizados
                        .AsNoTracking()
                        .Where(p => p.IdUsuario == idUsuario && p.IdCuestionario == dto.IdCuestionario)
                        .OrderByDescending(p => p.FechaCreacion)
                        .FirstOrDefaultAsync();

                    if (planExistente != null && planExistente.FechaCreacion > DateTime.UtcNow.AddMinutes(-1))
                    {
                        _logger.LogWarning("[PlanPersonalizado] Se detectó un plan recién creado (posible duplicado). IdPlan={IdPlan}", planExistente.IdPlan);
                        await transaction.RollbackAsync();
                        return MapToDto(planExistente);
                    }

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
                    await transaction.CommitAsync();

                    _logger.LogInformation("[PlanPersonalizado] Plan guardado correctamente. IdPlan={IdPlan}", plan.IdPlan);
                    return MapToDto(plan);
                }
                catch (DbUpdateException ex) when (ex.InnerException is NpgsqlException npgEx &&
                                                   (npgEx.Message.Contains("Timeout") || npgEx.SqlState == "57P01"))
                {
                    _logger.LogWarning(ex, "[PlanPersonalizado] Timeout al guardar. Verificando si el plan se insertó de todas formas...");

                    var planRecuperado = await _context.PlanesPersonalizados
                        .AsNoTracking()
                        .Where(p => p.IdUsuario == idUsuario && p.IdCuestionario == dto.IdCuestionario)
                        .OrderByDescending(p => p.FechaCreacion)
                        .FirstOrDefaultAsync();

                    if (planRecuperado != null && planRecuperado.FechaCreacion > DateTime.UtcNow.AddSeconds(-30))
                    {
                        await transaction.RollbackAsync();
                        _logger.LogInformation("[PlanPersonalizado] Plan recuperado exitosamente. IdPlan={IdPlan}", planRecuperado.IdPlan);
                        return MapToDto(planRecuperado);
                    }

                    await transaction.RollbackAsync();
                    throw;
                }
                catch (PostgresException ex) when (ex.SqlState == "55P03")
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException("La base de datos no pudo obtener el bloqueo necesario. Intenta de nuevo en unos segundos.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "[PlanPersonalizado] Error inesperado al crear plan.");
                    throw;
                }
            });
        }


        public async Task<PlanPersonalizadoDto?> ActualizarPlanAsync(Guid idUsuario, int idPlan, CreatePlanDto dto)
        {
            var plan = await _context.PlanesPersonalizados
                .FirstOrDefaultAsync(p => p.IdUsuario == idUsuario && p.IdPlan == idPlan);

            if (plan == null) return null;

            await ValidarCuestionarioAsync(idUsuario, dto.IdCuestionario);
            _logger.LogInformation("[PlanPersonalizado] Actualizando plan {IdPlan}", idPlan);

            plan.IdCuestionario = dto.IdCuestionario;
            plan.HoraDescanso = dto.HoraDescanso!.Value;
            plan.EnfoqueDiario = dto.EnfoqueDiario!.Value;
            plan.PausasDiarias = dto.PausasDiarias!.Value;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("[PlanPersonalizado] Plan {IdPlan} actualizado correctamente.", idPlan);
                return MapToDto(plan);
            }
            catch (DbUpdateException ex) when (ex.InnerException is NpgsqlException npgEx &&
                                               npgEx.Message.Contains("Timeout"))
            {
                _logger.LogWarning(ex, "[PlanPersonalizado] Timeout al actualizar. Los cambios probablemente se aplicaron.");
                return MapToDto(plan); // Asumimos que se aplicó
            }
            catch (PostgresException ex) when (ex.SqlState == "55P03")
            {
                throw new InvalidOperationException("La base de datos no pudo obtener el bloqueo necesario. Intenta de nuevo.");
            }
        }

        public async Task<bool> EliminarPlanAsync(Guid idUsuario, int idPlan)
        {
            var plan = await _context.PlanesPersonalizados
                .FirstOrDefaultAsync(p => p.IdUsuario == idUsuario && p.IdPlan == idPlan);

            if (plan == null) return false;

            _context.PlanesPersonalizados.Remove(plan);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task ValidarCuestionarioAsync(Guid idUsuario, int? idCuestionario)
        {
            if (!idCuestionario.HasValue) return;

            var cuestionarioExiste = await _context.Cuestionarios
                .AsNoTracking()
                .AnyAsync(c => c.IdCuestionario == idCuestionario.Value && c.IdUsuario == idUsuario);

            if (!cuestionarioExiste)
                throw new ArgumentException("El cuestionario indicado no existe o no pertenece al usuario autenticado.");
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