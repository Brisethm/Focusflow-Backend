using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Services
{
    public interface IRecordatorioService
    {
        Task<IEnumerable<RecordatorioDto>> ObtenerRecordatoriosAsync(Guid idUsuario);
        Task<RecordatorioDto?> ObtenerRecordatorioPorIdAsync(Guid idUsuario, int idRecordatorio);
        Task<RecordatorioDto> CrearRecordatorioAsync(Guid idUsuario, RecordatorioDto dto);
        Task<RecordatorioDto?> ActualizarRecordatorioAsync(Guid idUsuario, int idRecordatorio, RecordatorioDto dto);
        Task<bool> EliminarRecordatorioAsync(Guid idUsuario, int idRecordatorio);
    }
}
