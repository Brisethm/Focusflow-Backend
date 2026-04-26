using FocusFlowAPI.Models;
using FocusFlowAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FocusFlowAPI.Services
{
    public class RegistroEmocionalService
    {
        private readonly UsuarioContext _context;
        private readonly ILogger<RegistroEmocionalService> _logger;

        public RegistroEmocionalService(UsuarioContext context, ILogger<RegistroEmocionalService> logger)
        {
            _context = context;
            _logger = logger;
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
                Fecha = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };

            _context.RegistrosEmocionales.Add(registro);
            await _context.SaveChangesAsync();
            return MapToResponseDto(registro);
        }

        public async Task<RegistroEmocionalResponseDto?> ActualizarRegistroAsync(Guid sub, int idRegistro, RegistroEmocionalDto dto)
        {
            _logger.LogInformation("[RegistroEmocional] Actualizando registro {IdRegistro}", idRegistro);

            var filas = await _context.RegistrosEmocionales
                .Where(r => r.IdUsuario == sub && r.IdRegistro == idRegistro)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.EstadoAnimo, dto.EstadoAnimo ?? string.Empty)
                    .SetProperty(r => r.NivelEnergia, dto.NivelEnergia)
                    .SetProperty(r => r.NotaOpcional, dto.NotaOpcional)
                );

            if (filas == 0)
                return null;

            _logger.LogInformation("[RegistroEmocional] Registro {IdRegistro} actualizado correctamente.", idRegistro);

            return new RegistroEmocionalResponseDto
            {
                IdRegistro = idRegistro,
                IdUsuario = sub,
                EstadoAnimo = dto.EstadoAnimo ?? string.Empty,
                NivelEnergia = dto.NivelEnergia,
                NotaOpcional = dto.NotaOpcional
            };
        }

        public async Task<bool> EliminarRegistroAsync(Guid sub, int idRegistro)
        {
            var filas = await _context.RegistrosEmocionales
                .Where(r => r.IdUsuario == sub && r.IdRegistro == idRegistro)
                .ExecuteDeleteAsync();

            return filas > 0;
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