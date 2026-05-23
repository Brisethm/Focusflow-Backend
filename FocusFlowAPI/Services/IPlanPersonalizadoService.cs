using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Services
{
    public interface IPlanPersonalizadoService
    {
        Task<IEnumerable<PlanPersonalizadoDto>> ObtenerPlanesAsync(Guid idUsuario);
        Task<PlanPersonalizadoDto?> ObtenerPlanPorIdAsync(Guid idUsuario, int idPlan);
        Task<PlanPersonalizadoDto> CrearPlanAsync(Guid idUsuario, CreatePlanDto dto);
        Task<PlanPersonalizadoDto?> ActualizarPlanAsync(Guid idUsuario, int idPlan, CreatePlanDto dto);
        Task<bool> EliminarPlanAsync(Guid idUsuario, int idPlan);
    }
}