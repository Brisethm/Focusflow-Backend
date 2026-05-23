namespace FocusFlowAPI.DTOs
{
    public class PlanPersonalizadoDto
    {
        public int IdPlan { get; set; }
        public Guid IdUsuario { get; set; }
        public int? IdCuestionario { get; set; }
        public TimeOnly? HoraDescanso { get; set; }
        public int? EnfoqueDiario { get; set; }
        public int? PausasDiarias { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
