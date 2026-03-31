namespace FocusFlowAPI.DTOs
{
    public class RecordatorioDto
    {
        public required string Mensaje { get; set; }
        public DateTime ? FechaHora { get; set; }
        public string ? Tipo { get; set; }
    }
}
