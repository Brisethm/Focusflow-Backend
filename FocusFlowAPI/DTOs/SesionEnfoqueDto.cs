namespace FocusFlowAPI.DTOs
{
    public class SesionEnfoqueDto
    {
        public int? IdSesion { get; set; }

        public required int DuracionMinutos { get; set; }

        public required string Tipo { get; set; }

        public DateTime? Fecha { get; set; }
    }
}