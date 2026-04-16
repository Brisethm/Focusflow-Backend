using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FocusFlowAPI.Services
{
    public class TareaService
    {
        private readonly UsuarioContext _context;

        public TareaService(UsuarioContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TareaDto>> ObtenerTareasAsync(Guid idUsuario)
        {
            return await _context.Tareas
                .AsNoTracking()
                .Where(t => t.IdUsuario == idUsuario)
                .OrderByDescending(t => t.FechaCreacion)
                .Select(t => MapToDto(t))
                .ToListAsync();
        }

        public async Task<TareaDto?> ObtenerTareaPorIdAsync(Guid idUsuario, int idTarea)
        {
            var tarea = await _context.Tareas
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.IdUsuario == idUsuario && t.IdTarea == idTarea);

            return tarea == null ? null : MapToDto(tarea);
        }

        public async Task<TareaDto> CrearTareaAsync(Guid idUsuario, TareaDto dto)
        {
            var tarea = new Tarea
            {
                IdUsuario = idUsuario,
                Titulo = dto.Titulo,
                Prioridad = dto.Prioridad,
                NivelEsfuerzo = dto.NivelEsfuerzo,
                Estado = dto.Estado,
                FechaLimite = dto.FechaLimite
            };

            _context.Tareas.Add(tarea);
            await _context.SaveChangesAsync();

            return MapToDto(tarea);
        }

        public async Task<TareaDto?> ActualizarTareaAsync(Guid idUsuario, int idTarea, TareaDto dto)
        {
            var tarea = await _context.Tareas
                .FirstOrDefaultAsync(t => t.IdUsuario == idUsuario && t.IdTarea == idTarea);

            if (tarea == null)
                return null;

            tarea.Titulo = dto.Titulo;
            tarea.Prioridad = dto.Prioridad;
            tarea.NivelEsfuerzo = dto.NivelEsfuerzo;
            tarea.Estado = dto.Estado;
            tarea.FechaLimite = dto.FechaLimite;

            await _context.SaveChangesAsync();

            return MapToDto(tarea);
        }

        public async Task<bool> EliminarTareaAsync(Guid idUsuario, int idTarea)
        {
            var tarea = await _context.Tareas
                .FirstOrDefaultAsync(t => t.IdUsuario == idUsuario && t.IdTarea == idTarea);

            if (tarea == null)
                return false;

            _context.Tareas.Remove(tarea);
            await _context.SaveChangesAsync();
            return true;
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
                FechaCreacion = tarea.FechaCreacion,
                FechaLimite = tarea.FechaLimite
            };
        }
    }
}
