using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FocusFlowAPI.Services
{
    public class TransaccionService
    {
        private readonly UsuarioContext _context;

        public TransaccionService(UsuarioContext context)
        {
            _context = context;
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
                Fecha = DateTime.UtcNow
            };

            _context.Transacciones.Add(transaccion);
            await _context.SaveChangesAsync();

            return MapToDto(transaccion);
        }

        public async Task<TransaccionDto?> ActualizarTransaccionAsync(Guid idUsuario, int idTransaccion, TransaccionDto dto)
        {
            var transaccion = await _context.Transacciones
                .FirstOrDefaultAsync(t => t.IdUsuario == idUsuario && t.IdTransaccion == idTransaccion);

            if (transaccion == null)
                return null;

            transaccion.Monto = dto.Monto;
            transaccion.Tipo = dto.Tipo;
            transaccion.Categoria = dto.Categoria;
            transaccion.Descripcion = dto.Descripcion;

            await _context.SaveChangesAsync();

            return MapToDto(transaccion);
        }

        public async Task<bool> EliminarTransaccionAsync(Guid idUsuario, int idTransaccion)
        {
            var transaccion = await _context.Transacciones
                .FirstOrDefaultAsync(t => t.IdUsuario == idUsuario && t.IdTransaccion == idTransaccion);

            if (transaccion == null)
                return false;

            _context.Transacciones.Remove(transaccion);
            await _context.SaveChangesAsync();
            return true;
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
