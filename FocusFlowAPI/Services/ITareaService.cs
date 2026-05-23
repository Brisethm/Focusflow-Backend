using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Services
{
    public interface ITareaService
    {
        Task<IEnumerable<TareaDto>> ObtenerTareasAsync(Guid idUsuario);
        Task<TareaDto?> ObtenerTareaPorIdAsync(Guid idUsuario, int idTarea);
        Task<TareaDto> CrearTareaAsync(Guid idUsuario, TareaRequestDto dto);
        Task<TareaDto?> ActualizarTareaAsync(Guid idUsuario, int idTarea, TareaRequestDto dto);
        Task<bool> EliminarTareaAsync(Guid idUsuario, int idTarea);
    }
}