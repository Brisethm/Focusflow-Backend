using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Services
{
    public interface IPerfilUsuarioService
    {
        Task<PerfilUsuarioDto?> ObtenerPerfilAsync(Guid idUsuario);
        Task<PerfilUsuarioDto> CrearPerfilAsync(Guid idUsuario, PerfilUsuarioDto dto);
        Task<PerfilUsuarioDto?> ActualizarPerfilAsync(Guid idUsuario, PerfilUsuarioDto dto);
        Task<bool> EliminarPerfilAsync(Guid idUsuario);
    }
}