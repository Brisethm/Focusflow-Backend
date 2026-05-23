using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FocusFlowAPI.Models
{
    [Table("recordatorios")]
    public class Recordatorio
    {
        [Key]
        [Column("id_recordatorio")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdRecordatorio { get; set; }

        [Required]
        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Required]
        [Column("mensaje")]
        public string? Mensaje { get; set; }

        [Required]
        [Column("fecha_hora")]
        public DateTime FechaHora { get; set; }

        [MaxLength(50)]
        [Column("tipo")]
        public string? Tipo { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;
    }
}
