using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FocusFlowAPI.Services
{
    public class RecordatorioService
    {
        private readonly UsuarioContext _context;
        private readonly ILogger<RecordatorioService> _logger;

        public RecordatorioService(UsuarioContext context, ILogger<RecordatorioService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<RecordatorioDto>> ObtenerRecordatoriosAsync(Guid idUsuario)
        {
            return await _context.Recordatorios
                .AsNoTracking()
                .Where(r => r.IdUsuario == idUsuario)
                .OrderBy(r => r.FechaHora)
                .Select(r => MapToDto(r))
                .ToListAsync();
        }
        public async Task<RecordatorioDto?> ObtenerRecordatorioPorIdAsync(Guid idUsuario, int idRecordatorio)
        {
            var recordatorio = await _context.Recordatorios
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdUsuario == idUsuario && r.IdRecordatorio == idRecordatorio);

            return recordatorio == null ? null : MapToDto(recordatorio);
        }

        public async Task<RecordatorioDto> CrearRecordatorioAsync(Guid idUsuario, RecordatorioDto dto)
        {
            if (!dto.FechaHora.HasValue)
                throw new ArgumentException("El campo FechaHora es obligatorio.");

            var recordatorio = new Recordatorio
            {
                IdUsuario = idUsuario,
                Mensaje = dto.Mensaje,
                FechaHora = DateTime.SpecifyKind(dto.FechaHora.Value, DateTimeKind.Utc), // ✅ SpecifyKind
                Tipo = dto.Tipo,
                Activo = dto.Activo
            };

            _context.Recordatorios.Add(recordatorio);
            await _context.SaveChangesAsync();
            return MapToDto(recordatorio);
        }

        public async Task<RecordatorioDto?> ActualizarRecordatorioAsync(Guid idUsuario, int idRecordatorio, RecordatorioDto dto)
        {
            if (!dto.FechaHora.HasValue)
                throw new ArgumentException("El campo FechaHora es obligatorio.");

            var fechaHora = DateTime.SpecifyKind(dto.FechaHora.Value, DateTimeKind.Utc); // ✅ SpecifyKind

            _logger.LogInformation("[Recordatorio] Actualizando recordatorio {IdRecordatorio}", idRecordatorio);

            var filas = await _context.Recordatorios
                .Where(r => r.IdUsuario == idUsuario && r.IdRecordatorio == idRecordatorio)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.Mensaje, dto.Mensaje)
                    .SetProperty(r => r.FechaHora, fechaHora)
                    .SetProperty(r => r.Tipo, dto.Tipo)
                    .SetProperty(r => r.Activo, dto.Activo)
                );

            if (filas == 0)
                return null;

            _logger.LogInformation("[Recordatorio] Recordatorio {IdRecordatorio} actualizado correctamente.", idRecordatorio);

            return new RecordatorioDto
            {
                IdRecordatorio = idRecordatorio,
                Mensaje = dto.Mensaje ?? string.Empty,
                FechaHora = fechaHora,
                Tipo = dto.Tipo,
                Activo = dto.Activo
            };
        }

        public async Task<bool> EliminarRecordatorioAsync(Guid idUsuario, int idRecordatorio)
        {
            var filas = await _context.Recordatorios
                .Where(r => r.IdUsuario == idUsuario && r.IdRecordatorio == idRecordatorio)
                .ExecuteDeleteAsync();

            return filas > 0;
        }

        private static RecordatorioDto MapToDto(Recordatorio recordatorio)
        {
            return new RecordatorioDto
            {
                IdRecordatorio = recordatorio.IdRecordatorio,
                Mensaje = recordatorio.Mensaje ?? string.Empty,
                FechaHora = recordatorio.FechaHora,
                Tipo = recordatorio.Tipo,
                Activo = recordatorio.Activo
            };
        }
    }
}