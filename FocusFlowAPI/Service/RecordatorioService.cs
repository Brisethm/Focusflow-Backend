using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FocusFlowAPI.Services
{
    public class RecordatorioService
    {
        private readonly UsuarioContext _context;

        public RecordatorioService(UsuarioContext context)
        {
            _context = context;
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
                FechaHora = dto.FechaHora.Value,
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

            var recordatorio = await _context.Recordatorios
                .FirstOrDefaultAsync(r => r.IdUsuario == idUsuario && r.IdRecordatorio == idRecordatorio);

            if (recordatorio == null)
                return null;

            recordatorio.Mensaje = dto.Mensaje;
            recordatorio.FechaHora = dto.FechaHora.Value;
            recordatorio.Tipo = dto.Tipo;
            recordatorio.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return MapToDto(recordatorio);
        }

        public async Task<bool> EliminarRecordatorioAsync(Guid idUsuario, int idRecordatorio)
        {
            var recordatorio = await _context.Recordatorios
                .FirstOrDefaultAsync(r => r.IdUsuario == idUsuario && r.IdRecordatorio == idRecordatorio);

            if (recordatorio == null)
                return false;

            _context.Recordatorios.Remove(recordatorio);
            await _context.SaveChangesAsync();
            return true;
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
