namespace FocusFlowAPI.DTOs
{
    public class TicketDto
    {
        public int IdTicket { get; set; }
        public string Asunto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Prioridad { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public Guid? IdAsignado { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class CreateTicketDto
    {
        public string Asunto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Categoria { get; set; } = "general";
        public string Prioridad { get; set; } = "low";
    }
}