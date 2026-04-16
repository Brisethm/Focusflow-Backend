using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FocusFlowAPI.Models
{
    [Table("tareas")]
    public class Tarea
    {
        [Key]
        [Column("id_tarea")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdTarea { get; set; }

        [Required]
        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("titulo")]
        public required string Titulo { get; set; }

        [MaxLength(50)]
        [Column("prioridad")]
        public required string Prioridad { get; set; }

        [MaxLength(50)]
        [Column("nivel_esfuerzo")]
        public required string NivelEsfuerzo { get; set; }

        [MaxLength(50)]
        [Column("estado")]
        public required string Estado { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Column("fecha_limite")]
        public DateTime? FechaLimite { get; set; }
    }
}
