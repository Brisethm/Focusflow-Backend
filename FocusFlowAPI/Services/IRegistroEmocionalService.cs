using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Services
{
    public interface IRegistroEmocionalService
    {
        Task<IEnumerable<RegistroEmocionalResponseDto>> ObtenerRegistrosAsync(Guid idUsuario);
        Task<RegistroEmocionalResponseDto?> ObtenerRegistroPorIdAsync(Guid idUsuario, int idRegistro);
        Task<RegistroEmocionalResponseDto> CrearRegistroAsync(Guid idUsuario, RegistroEmocionalDto dto);
        Task<RegistroEmocionalResponseDto?> ActualizarRegistroAsync(Guid idUsuario, int idRegistro, RegistroEmocionalDto dto);
        Task<bool> EliminarRegistroAsync(Guid idUsuario, int idRegistro);
    }
}