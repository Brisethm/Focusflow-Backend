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

        public IEnumerable<RegistroEmocional> ObtenerRegistros(Guid sub)
        {
            return _context.RegistrosEmocionales.Where(r => r.IdUsuario == sub).ToList();
        }

        public RegistroEmocional CrearRegistro(Guid sub, RegistroEmocionalDto dto)
        {
            var registro = new RegistroEmocional
            {
                IdUsuario = sub,
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
