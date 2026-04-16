using FocusFlowAPI.Models;
using FocusFlowAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FocusFlowAPI.Services
{
    public class RegistroEmocionalService
    {
        private readonly UsuarioContext _context;

        public RegistroEmocionalService(UsuarioContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RegistroEmocionalResponseDto>> ObtenerRegistrosAsync(Guid sub)
        {
            return await _context.RegistrosEmocionales
                .AsNoTracking()
                .Where(r => r.IdUsuario == sub)
                .OrderByDescending(r => r.Fecha)
                .Select(r => new RegistroEmocionalResponseDto
                {
                    IdRegistro = r.IdRegistro,
                    IdUsuario = r.IdUsuario,
                    EstadoAnimo = r.EstadoAnimo,
                    NivelEnergia = r.NivelEnergia,
                    NotaOpcional = r.NotaOpcional,
                    Fecha = r.Fecha
                })
                .ToListAsync();
        }

        public async Task<RegistroEmocionalResponseDto?> ObtenerRegistroPorIdAsync(Guid sub, int idRegistro)
        {
            var registro = await _context.RegistrosEmocionales
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdUsuario == sub && r.IdRegistro == idRegistro);

            return registro == null ? null : MapToResponseDto(registro);
        }

        public async Task<RegistroEmocionalResponseDto> CrearRegistroAsync(Guid sub, RegistroEmocionalDto dto)
        {
            var registro = new RegistroEmocional
            {
                IdUsuario = sub,
                EstadoAnimo = dto.EstadoAnimo ?? string.Empty,
                NivelEnergia = dto.NivelEnergia,
                NotaOpcional = dto.NotaOpcional,
                Fecha = DateTime.UtcNow
            };

            _context.RegistrosEmocionales.Add(registro);
            await _context.SaveChangesAsync();

            return MapToResponseDto(registro);
        }

        public async Task<RegistroEmocionalResponseDto?> ActualizarRegistroAsync(Guid sub, int idRegistro, RegistroEmocionalDto dto)
        {
            var registro = await _context.RegistrosEmocionales
                .FirstOrDefaultAsync(r => r.IdUsuario == sub && r.IdRegistro == idRegistro);

            if (registro == null)
                return null;

            registro.EstadoAnimo = dto.EstadoAnimo ?? string.Empty;
            registro.NivelEnergia = dto.NivelEnergia;
            registro.NotaOpcional = dto.NotaOpcional;

            await _context.SaveChangesAsync();

            return MapToResponseDto(registro);
        }

        public async Task<bool> EliminarRegistroAsync(Guid sub, int idRegistro)
        {
            var registro = await _context.RegistrosEmocionales
                .FirstOrDefaultAsync(r => r.IdUsuario == sub && r.IdRegistro == idRegistro);

            if (registro == null)
                return false;

            _context.RegistrosEmocionales.Remove(registro);
            await _context.SaveChangesAsync();
            return true;
        }

        private static RegistroEmocionalResponseDto MapToResponseDto(RegistroEmocional registro)
        {
            return new RegistroEmocionalResponseDto
            {
                IdRegistro = registro.IdRegistro,
                IdUsuario = registro.IdUsuario,
                EstadoAnimo = registro.EstadoAnimo,
                NivelEnergia = registro.NivelEnergia,
                NotaOpcional = registro.NotaOpcional,
                Fecha = registro.Fecha
            };
        }
    }
}
