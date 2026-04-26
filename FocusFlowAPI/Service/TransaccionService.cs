using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FocusFlowAPI.Services
{
    public class TransaccionService
    {
        private readonly UsuarioContext _context;
        private readonly ILogger<TransaccionService> _logger;

        public TransaccionService(UsuarioContext context, ILogger<TransaccionService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<IEnumerable<TransaccionDto>> ObtenerTransaccionesAsync(Guid idUsuario)
        {
            return await _context.Transacciones
                .AsNoTracking()
                .Where(t => t.IdUsuario == idUsuario)
                .OrderByDescending(t => t.Fecha)
                .Select(t => MapToDto(t))
                .ToListAsync();
        }

        public async Task<TransaccionDto?> ObtenerTransaccionPorIdAsync(Guid idUsuario, int idTransaccion)
        {
            var transaccion = await _context.Transacciones
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.IdUsuario == idUsuario && t.IdTransaccion == idTransaccion);

            return transaccion == null ? null : MapToDto(transaccion);
        }
        public async Task<TransaccionDto> CrearTransaccionAsync(Guid idUsuario, TransaccionDto dto)
        {
            var transaccion = new Transaccion
            {
                IdUsuario = idUsuario,
                Monto = dto.Monto,
                Tipo = dto.Tipo,
                Categoria = dto.Categoria,
                Descripcion = dto.Descripcion,
                Fecha = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };

            _context.Transacciones.Add(transaccion);
            await _context.SaveChangesAsync();
            return MapToDto(transaccion);
        }

        public async Task<TransaccionDto?> ActualizarTransaccionAsync(Guid idUsuario, int idTransaccion, TransaccionDto dto)
        {
            _logger.LogInformation("[Transaccion] Actualizando transacción {IdTransaccion}", idTransaccion);

            var filas = await _context.Transacciones
                .Where(t => t.IdUsuario == idUsuario && t.IdTransaccion == idTransaccion)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.Monto, dto.Monto)
                    .SetProperty(t => t.Tipo, dto.Tipo)
                    .SetProperty(t => t.Categoria, dto.Categoria)
                    .SetProperty(t => t.Descripcion, dto.Descripcion)
                );

            if (filas == 0)
                return null;

            _logger.LogInformation("[Transaccion] Transacción {IdTransaccion} actualizada correctamente.", idTransaccion);

            return new TransaccionDto
            {
                IdTransaccion = idTransaccion,
                Monto = dto.Monto,
                Tipo = dto.Tipo,
                Categoria = dto.Categoria,
                Descripcion = dto.Descripcion
            };
        }

        public async Task<bool> EliminarTransaccionAsync(Guid idUsuario, int idTransaccion)
        {
            var filas = await _context.Transacciones
                .Where(t => t.IdUsuario == idUsuario && t.IdTransaccion == idTransaccion)
                .ExecuteDeleteAsync();

            return filas > 0;
        }
        private static TransaccionDto MapToDto(Transaccion transaccion)
        {
            return new TransaccionDto
            {
                IdTransaccion = transaccion.IdTransaccion,
                Monto = transaccion.Monto,
                Tipo = transaccion.Tipo,
                Categoria = transaccion.Categoria,
                Descripcion = transaccion.Descripcion,
                Fecha = transaccion.Fecha
            };
        }
    }
}