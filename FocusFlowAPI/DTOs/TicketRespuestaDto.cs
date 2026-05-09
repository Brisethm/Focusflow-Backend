namespace FocusFlowAPI.DTOs
{
    public class TicketRespuestaDto
    {
        public int IdRespuesta { get; set; }
        public Guid IdAutor { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public bool EsSoporte { get; set; }
    }

    public class CreateRespuestaDto
    {
        public string Mensaje { get; set; } = string.Empty;
    }
}