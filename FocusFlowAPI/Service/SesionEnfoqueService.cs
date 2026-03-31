using FocusFlowAPI.Models;
using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Services
{
    public class SesionEnfoqueService
    {
        private readonly UsuarioContext _context;

        public SesionEnfoqueService(UsuarioContext context)
        {
            _context = context;
        }

        public IEnumerable<SesionEnfoque> ObtenerSesiones(Guid idUsuario)
        {
            return _context.SesionesEnfoque.Where(s => s.IdUsuario == idUsuario).ToList();
        }

        public SesionEnfoque CrearSesion(Guid idUsuario, SesionEnfoqueDto dto)
        {
            var sesion = new SesionEnfoque
            {
                IdUsuario = idUsuario,
                DuracionMinutos = dto.DuracionMinutos,
                Tipo = dto.Tipo,
                Fecha = DateTime.UtcNow
            };

            _context.SesionesEnfoque.Add(sesion);
            _context.SaveChanges();

            return sesion;
        }
    }
}
