using FocusFlowAPI.Models;
using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Services
{
    public class TareaService
    {
        private readonly UsuarioContext _context;

        public TareaService(UsuarioContext context)
        {
            _context = context;
        }

        public IEnumerable<Tarea> ObtenerTareas(Guid idUsuario)
        {
            return _context.Tareas.Where(t => t.IdUsuario == idUsuario).ToList();
        }

        public Tarea CrearTarea(Guid idUsuario, TareaDto dto)
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
            _context.SaveChanges();

            return tarea;
        }
    }
}
