namespace FocusFlowAPI.DTOs
{
    public class RecordatorioDto
    {
        public int IdRecordatorio { get; set; }
        public required string Mensaje { get; set; }
        public DateTime? FechaHora { get; set; }
        public string? Tipo { get; set; }
        public bool Activo { get; set; } = true;
    }
}
