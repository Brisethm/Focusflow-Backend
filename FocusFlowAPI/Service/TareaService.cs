using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FocusFlowAPI.Services
{
    public class TareaService
    {
        private readonly UsuarioContext _context;
        private readonly ILogger<TareaService> _logger;

        public TareaService(UsuarioContext context, ILogger<TareaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<TareaDto>> ObtenerTareasAsync(Guid idUsuario)
        {
            return await _context.Tareas
                .AsNoTracking()
                .Where(t => t.IdUsuario == idUsuario)
                .OrderByDescending(t => t.FechaCreacion)
                .Select(t => new TareaDto
                {
                    IdTarea = t.IdTarea,
                    Titulo = t.Titulo,
                    Prioridad = t.Prioridad,
                    NivelEsfuerzo = t.NivelEsfuerzo,
                    Estado = t.Estado,
                    FechaCreacion = t.FechaCreacion,
                    FechaLimite = t.FechaLimite
                })
                .ToListAsync();
        }

        public async Task<TareaDto?> ObtenerTareaPorIdAsync(Guid idUsuario, int idTarea)
        {
            return await _context.Tareas
            .AsNoTracking()
            .Where(t => t.IdUsuario == idUsuario && t.IdTarea == idTarea)
            .Select(t => new TareaDto
            {
                IdTarea = t.IdTarea,
                Titulo = t.Titulo,
                Prioridad = t.Prioridad,
                NivelEsfuerzo = t.NivelEsfuerzo,
                Estado = t.Estado,
                FechaCreacion = t.FechaCreacion,
                FechaLimite = t.FechaLimite
            })
            .FirstOrDefaultAsync();
        }

        public async Task<TareaDto> CrearTareaAsync(Guid idUsuario, TareaRequestDto dto)
        {
            var tarea = new Tarea
            {
                IdUsuario = idUsuario,
                Titulo = dto.Titulo,
                Prioridad = dto.Prioridad,
                NivelEsfuerzo = dto.NivelEsfuerzo,
                Estado = dto.Estado,
                FechaLimite = dto.FechaLimite.HasValue
                    ? DateTime.SpecifyKind(dto.FechaLimite.Value, DateTimeKind.Utc)
                    : null
            };

            _context.Tareas.Add(tarea);
            await _context.SaveChangesAsync();

            return MapToDto(tarea);
        }

        public async Task<TareaDto?> ActualizarTareaAsync(Guid idUsuario, int idTarea, TareaRequestDto dto)
        {
            _logger.LogInformation("[ActualizarTarea] Inicio — idUsuario={IdUsuario}, idTarea={IdTarea}", idUsuario, idTarea);

            var fechaLimite = dto.FechaLimite.HasValue
                ? DateTime.SpecifyKind(dto.FechaLimite.Value, DateTimeKind.Utc)
                : (DateTime?)null;

            var filas = await _context.Tareas
                .Where(t => t.IdUsuario == idUsuario && t.IdTarea == idTarea)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.Titulo, dto.Titulo)
                    .SetProperty(t => t.Prioridad, dto.Prioridad)
                    .SetProperty(t => t.NivelEsfuerzo, dto.NivelEsfuerzo)
                    .SetProperty(t => t.Estado, dto.Estado)
                    .SetProperty(t => t.FechaLimite, fechaLimite)
                );

            _logger.LogInformation("[ActualizarTarea] Filas afectadas: {Filas}", filas);

            if (filas == 0)
                return null;

            return new TareaDto
            {
                IdTarea = idTarea,
                Titulo = dto.Titulo,
                Prioridad = dto.Prioridad,
                NivelEsfuerzo = dto.NivelEsfuerzo,
                Estado = dto.Estado,
                FechaCreacion = DateTime.UtcNow, // El frontend ya lo tiene, no cambia
                FechaLimite = fechaLimite
            };
        }

        public async Task<bool> EliminarTareaAsync(Guid idUsuario, int idTarea)
        {
            var filas = await _context.Tareas
                .Where(t => t.IdUsuario == idUsuario && t.IdTarea == idTarea)
                .ExecuteDeleteAsync();

            return filas > 0;
        }

        private static TareaDto MapToDto(Tarea tarea)
        {
            return new TareaDto
            {
                IdTarea = tarea.IdTarea,
                Titulo = tarea.Titulo,
                Prioridad = tarea.Prioridad,
                NivelEsfuerzo = tarea.NivelEsfuerzo,
                Estado = tarea.Estado,
                FechaCreacion = DateTime.SpecifyKind(tarea.FechaCreacion, DateTimeKind.Utc),
                FechaLimite = tarea.FechaLimite.HasValue
                    ? DateTime.SpecifyKind(tarea.FechaLimite.Value, DateTimeKind.Utc)
                    : null
            };
        }
    }
}
