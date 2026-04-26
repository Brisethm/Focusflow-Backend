using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FocusFlowAPI.Services
{
    public class SesionEnfoqueService
    {
        private readonly UsuarioContext _context;
        private readonly ILogger<SesionEnfoqueService> _logger;

        public SesionEnfoqueService(UsuarioContext context, ILogger<SesionEnfoqueService> logger){
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<SesionEnfoqueDto>> ObtenerSesionesAsync(Guid idUsuario)
        {
            return await _context.SesionesEnfoque
                .AsNoTracking()
                .Where(s => s.IdUsuario == idUsuario)
                .OrderByDescending(s => s.Fecha)
                .Select(s => MapToDto(s))
                .ToListAsync();
        }
        public async Task<SesionEnfoqueDto?> ObtenerSesionPorIdAsync(Guid idUsuario, int idSesion)
        {
            var sesion = await _context.SesionesEnfoque
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.IdUsuario == idUsuario && s.IdSesion == idSesion);

            return sesion == null ? null : MapToDto(sesion);
        }

        public async Task<SesionEnfoqueDto> CrearSesionAsync(Guid idUsuario, SesionEnfoqueDto dto)
        {
            var sesion = new SesionEnfoque
            {
                IdUsuario = idUsuario,
                DuracionMinutos = dto.DuracionMinutos,
                Tipo = dto.Tipo,
                Fecha = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };

            _context.SesionesEnfoque.Add(sesion);
            await _context.SaveChangesAsync();
            return MapToDto(sesion);
        }
        public async Task<SesionEnfoqueDto?> ActualizarSesionAsync(Guid idUsuario, int idSesion, SesionEnfoqueDto dto)
        {
            _logger.LogInformation("[SesionEnfoque] Actualizando sesión {IdSesion}", idSesion);

            var filas = await _context.SesionesEnfoque
                .Where(s => s.IdUsuario == idUsuario && s.IdSesion == idSesion)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(s => s.DuracionMinutos, dto.DuracionMinutos)
                    .SetProperty(s => s.Tipo, dto.Tipo)
                );

            if (filas == 0)
                return null;

            _logger.LogInformation("[SesionEnfoque] Sesión {IdSesion} actualizada correctamente.", idSesion);

            return new SesionEnfoqueDto
            {
                IdSesion = idSesion,
                DuracionMinutos = dto.DuracionMinutos,
                Tipo = dto.Tipo
            };
        }
        public async Task<bool> EliminarSesionAsync(Guid idUsuario, int idSesion)
        {
            var filas = await _context.SesionesEnfoque
                .Where(s => s.IdUsuario == idUsuario && s.IdSesion == idSesion)
                .ExecuteDeleteAsync();

            return filas > 0;
        }

        private static SesionEnfoqueDto MapToDto(SesionEnfoque sesion)
        {
            return new SesionEnfoqueDto
            {
                IdSesion = sesion.IdSesion,
                DuracionMinutos = sesion.DuracionMinutos,
                Tipo = sesion.Tipo,
                Fecha = sesion.Fecha
            };
        }
    }
}