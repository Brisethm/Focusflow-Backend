using FocusFlowAPI.Models;
using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Services
{
    public class TransaccionService
    {
        private readonly UsuarioContext _context;

        public TransaccionService(UsuarioContext context)
        {
            _context = context;
        }

        public IEnumerable<Transaccion> ObtenerTransacciones(Guid idUsuario)
        {
            return _context.Transacciones.Where(t => t.IdUsuario == idUsuario).ToList();
        }

        public Transaccion CrearTransaccion(Guid idUsuario, TransaccionDto dto)
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
            _context.SaveChanges();

            return transaccion;
        }
    }
}
