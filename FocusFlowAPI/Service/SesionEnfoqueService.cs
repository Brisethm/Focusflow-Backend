using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FocusFlowAPI.Services
{
    public class SesionEnfoqueService
    {
        private readonly UsuarioContext _context;

        public SesionEnfoqueService(UsuarioContext context)
        {
            _context = context;
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
                Fecha = DateTime.UtcNow
            };

            _context.SesionesEnfoque.Add(sesion);
            await _context.SaveChangesAsync();

            return MapToDto(sesion);
        }

        public async Task<SesionEnfoqueDto?> ActualizarSesionAsync(Guid idUsuario, int idSesion, SesionEnfoqueDto dto)
        {
            var sesion = await _context.SesionesEnfoque
                .FirstOrDefaultAsync(s => s.IdUsuario == idUsuario && s.IdSesion == idSesion);

            if (sesion == null)
                return null;

            sesion.DuracionMinutos = dto.DuracionMinutos;
            sesion.Tipo = dto.Tipo;

            await _context.SaveChangesAsync();

            return MapToDto(sesion);
        }

        public async Task<bool> EliminarSesionAsync(Guid idUsuario, int idSesion)
        {
            var sesion = await _context.SesionesEnfoque
                .FirstOrDefaultAsync(s => s.IdUsuario == idUsuario && s.IdSesion == idSesion);

            if (sesion == null)
                return false;

            _context.SesionesEnfoque.Remove(sesion);
            await _context.SaveChangesAsync();
            return true;
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
