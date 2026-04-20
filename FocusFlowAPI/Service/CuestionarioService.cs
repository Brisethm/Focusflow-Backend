using FocusFlowAPI.Models;
using FocusFlowAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FocusFlowAPI.Services
{
    public class CuestionarioService
    {
        private readonly UsuarioContext _context;

        public CuestionarioService(UsuarioContext context)
        {
            _context = context;
        }

        // GET
        public async Task<IEnumerable<CuestionarioDto>> ObtenerCuestionariosAsync(Guid idUsuario)
        {
            return await _context.Cuestionarios
                .AsNoTracking()
                .Where(c => c.IdUsuario == idUsuario)
                .OrderByDescending(c => c.Fecha)
                .Select(c => new CuestionarioDto
                {
                    IdCuestionario = c.IdCuestionario,
                    PuntajeTotal = c.PuntajeTotal,
                    Completado = c.Completado,
                    NivelEstres = c.NivelEstres,
                    NivelEnergia = c.NivelEnergia,
                    NivelOrganizacion = c.NivelOrganizacion,
                    NivelProcrastinacion = c.NivelProcrastinacion,
                    Perfil = c.Perfil,

                    Respuestas = c.Respuestas.Select(r => new RespuestaDto
                    {
                        Pregunta = r.Pregunta,
                        Valor = r.Valor!,
                        Categoria = r.Categoria,
                        Puntaje = r.Puntaje
                    }).ToList()
                })
                .ToListAsync();
        }

        // POST
        public async Task<CuestionarioDto> CrearCuestionarioAsync(Guid idUsuario, CuestionarioDto dto)
        {
            if (dto.Completado == null)
                throw new ArgumentException("El campo Completado es obligatorio.");

            var cuestionario = new Cuestionario
            {
                IdUsuario = idUsuario,
                PuntajeTotal = dto.PuntajeTotal,
                Completado = dto.Completado.Value,
                NivelEstres = dto.NivelEstres,
                NivelEnergia = dto.NivelEnergia,
                NivelOrganizacion = dto.NivelOrganizacion,
                NivelProcrastinacion = dto.NivelProcrastinacion,
                Perfil = dto.Perfil,
                Fecha = DateTime.UtcNow
            };

            if (dto.Respuestas != null && dto.Respuestas.Any())
            {
                cuestionario.Respuestas = dto.Respuestas.Select(r => new RespuestaCuestionario
                {
                    IdUsuario = idUsuario,
                    Pregunta = r.Pregunta,
                    Valor = r.Valor,
                    Categoria = r.Categoria,
                    Puntaje = r.Puntaje
                }).ToList();
            }

            _context.Cuestionarios.Add(cuestionario);
            await _context.SaveChangesAsync();

            return new CuestionarioDto
            {
                IdCuestionario = cuestionario.IdCuestionario,
                PuntajeTotal = cuestionario.PuntajeTotal,
                Completado = cuestionario.Completado,
                NivelEstres = cuestionario.NivelEstres,
                NivelEnergia = cuestionario.NivelEnergia,
                NivelOrganizacion = cuestionario.NivelOrganizacion,
                NivelProcrastinacion = cuestionario.NivelProcrastinacion,
                Perfil = cuestionario.Perfil,
                Respuestas = cuestionario.Respuestas.Select(r => new RespuestaDto
                {
                    Pregunta = r.Pregunta,
                    Valor = r.Valor ?? string.Empty,
                    Categoria = r.Categoria,
                    Puntaje = r.Puntaje
                }).ToList()
            };
        }
    }
}
