using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Services
{
    public interface ITransaccionService
    {
        Task<IEnumerable<TransaccionDto>> ObtenerTransaccionesAsync(Guid idUsuario);
        Task<TransaccionDto?> ObtenerTransaccionPorIdAsync(Guid idUsuario, int idTransaccion);
        Task<TransaccionDto> CrearTransaccionAsync(Guid idUsuario, TransaccionDto dto);
        Task<TransaccionDto?> ActualizarTransaccionAsync(Guid idUsuario, int idTransaccion, TransaccionDto dto);
        Task<bool> EliminarTransaccionAsync(Guid idUsuario, int idTransaccion);
    }
}