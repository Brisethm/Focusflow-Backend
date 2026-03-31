using FocusFlowAPI.Models;
using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Services
{
    public class RecordatorioService
    {
        private readonly UsuarioContext _context;

        public RecordatorioService(UsuarioContext context)
        {
            _context = context;
        }

        public IEnumerable<Recordatorio> ObtenerRecordatorios(Guid idUsuario)
        {
            return _context.Recordatorios.Where(r => r.IdUsuario == idUsuario).ToList();
        }

        public Recordatorio CrearRecordatorio(Guid idUsuario, RecordatorioDto dto)
        {
            if (!dto.FechaHora.HasValue)
            {
                throw new ArgumentException("El campo FechaHora es obligatorio.");
            }
            var recordatorio = new Recordatorio
            {
                IdUsuario = idUsuario,
                Mensaje = dto.Mensaje,
                FechaHora = dto.FechaHora.Value,
                Tipo = dto.Tipo,
                Activo = true
            };

            _context.Recordatorios.Add(recordatorio);
            _context.SaveChanges();

            return recordatorio;
        }
    }
}
