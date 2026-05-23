using FocusFlowAPI.Models;
using FocusFlowAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FocusFlowAPI.Services
{
    public class CuestionarioService : ICuestionarioService
    {
        private readonly UsuarioContext _context;
        private readonly ILogger<CuestionarioService> _logger;

        public CuestionarioService(UsuarioContext context, ILogger<CuestionarioService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<CuestionarioDto>> ObtenerCuestionariosAsync(Guid idUsuario)
        {
            _logger.LogInformation("Obteniendo cuestionarios para el usuario: {IdUsuario}", idUsuario);

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
                        Valor = r.Valor ?? string.Empty,
                        Categoria = r.Categoria,
                        Puntaje = r.Puntaje
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<CuestionarioDto> CrearCuestionarioAsync(Guid idUsuario, CuestionarioDto dto)
        {
            _logger.LogInformation("Creando nuevo cuestionario para el usuario: {IdUsuario}", idUsuario);

            if (dto.Completado is null)
            {
                throw new ArgumentException("El campo Completado es obligatorio.");
            }

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
                Fecha = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                Respuestas = dto.Respuestas?.Select(r => new RespuestaCuestionario
                {
                    IdUsuario = idUsuario,
                    Pregunta = r.Pregunta,
                    Valor = r.Valor,
                    Categoria = r.Categoria,
                    Puntaje = r.Puntaje
                }).ToList() ?? []
            };

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