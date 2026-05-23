using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FocusFlowAPI.Models
{
    [Table("tickets")]
    public class Ticket
    {
        [Key]
        [Column("id_ticket")]
        public int IdTicket { get; set; }

        [Required]
        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Required]
        [StringLength(200)]
        [Column("asunto")]
        public string Asunto { get; set; } = string.Empty;

        [Required]
        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [Column("estado")]
        public string Estado { get; set; } = "open";

        [Column("prioridad")]
        public string Prioridad { get; set; } = "low";

        [Column("categoria")]
        public string Categoria { get; set; } = "general";

        [Column("id_asignado")]
        public Guid? IdAsignado { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Column("ultima_actualizacion")]
        public DateTime UltimaActualizacion { get; set; } = DateTime.UtcNow;
    }
}