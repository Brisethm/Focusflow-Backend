using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Services
{
    public interface ISesionEnfoqueService
    {
        Task<IEnumerable<SesionEnfoqueDto>> ObtenerSesionesAsync(Guid idUsuario);
        Task<SesionEnfoqueDto?> ObtenerSesionPorIdAsync(Guid idUsuario, int idSesion);
        Task<SesionEnfoqueDto> CrearSesionAsync(Guid idUsuario, SesionEnfoqueDto dto);
        Task<SesionEnfoqueDto?> ActualizarSesionAsync(Guid idUsuario, int idSesion, SesionEnfoqueDto dto);
        Task<bool> EliminarSesionAsync(Guid idUsuario, int idSesion);
    }
}