using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FocusFlowAPI.Models
{
    [Table("respuestas_tickets")]
    public class TicketRespuesta
    {
        [Key]
        [Column("id_respuesta")]
        public int IdRespuesta { get; set; }

        [Column("id_ticket")]
        public int IdTicket { get; set; }

        [Column("id_autor")]
        public Guid IdAutor { get; set; }

        [Column("mensaje")]
        public string Mensaje { get; set; } = string.Empty;

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
