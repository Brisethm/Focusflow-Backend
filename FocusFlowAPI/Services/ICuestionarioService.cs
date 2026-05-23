using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Services
{
    public interface ICuestionarioService
    {
        Task<IEnumerable<CuestionarioDto>> ObtenerCuestionariosAsync(Guid idUsuario);
        Task<CuestionarioDto> CrearCuestionarioAsync(Guid idUsuario, CuestionarioDto dto);
    }
}