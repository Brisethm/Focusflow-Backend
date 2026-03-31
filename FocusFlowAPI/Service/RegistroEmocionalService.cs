using FocusFlowAPI.Models;
using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Services
{
    public class RegistroEmocionalService
    {
        private readonly UsuarioContext _context;

        public RegistroEmocionalService(UsuarioContext context)
        {
            _context = context;
        }

        public IEnumerable<RegistroEmocional> ObtenerRegistros(Guid idUsuario)
        {
            return _context.RegistrosEmocionales.Where(r => r.IdUsuario == idUsuario).ToList();
        }

        public RegistroEmocional CrearRegistro(Guid idUsuario, RegistroEmocionalDto dto)
        {
            var registro = new RegistroEmocional
            {
                IdUsuario = idUsuario,
                EstadoAnimo = dto.EstadoAnimo,
                NivelEnergia = dto.NivelEnergia,
                NotaOpcional = dto.NotaOpcional,
                Fecha = DateTime.UtcNow
            };

            _context.RegistrosEmocionales.Add(registro);
            _context.SaveChanges();

            return registro;
        }
    }
}
