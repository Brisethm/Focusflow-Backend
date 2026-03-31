using FocusFlowAPI.Models;
using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Services
{
    public class CuestionarioService
    {
        private readonly UsuarioContext _context;

        public CuestionarioService(UsuarioContext context)
        {
            _context = context;
        }

        public IEnumerable<Cuestionario> ObtenerCuestionarios(Guid idUsuario)
        {
            return _context.Cuestionarios
                .Where(c => c.IdUsuario == idUsuario)
                .ToList();
        }

        public Cuestionario CrearCuestionario(Guid idUsuario, CuestionarioDto dto)
        {
             if (!dto.Completado.HasValue)
            {
                throw new ArgumentException("El campo Completado es obligatorio.");
            }
            var cuestionario = new Cuestionario
            {
                IdUsuario = idUsuario,
                PuntajeTotal = dto.PuntajeTotal,
                Completado = dto.Completado.Value,
                Fecha = DateTime.UtcNow
            };

            _context.Cuestionarios.Add(cuestionario);
            _context.SaveChanges();


            if (dto.Respuestas != null)
            {
                foreach (var r in dto.Respuestas)
                {
                    var respuesta = new RespuestaCuestionario
                    {
                        IdCuestionario = cuestionario.IdCuestionario,
                        Pregunta = r.Pregunta,
                        Valor = r.Valor
                    };
                    _context.RespuestasCuestionarios.Add(respuesta);
                }
                _context.SaveChanges();
            }

            return cuestionario;
        }
    }
}
