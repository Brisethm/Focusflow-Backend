namespace FocusFlowAPI.DTOs
{
    public class TareaDto
    {
        public int IdTarea { get; set; }
        public required string Titulo { get; set; }
        public required string Prioridad { get; set; }
        public required string NivelEsfuerzo { get; set; }
        public required string Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaLimite { get; set; }
    }
}
