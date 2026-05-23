using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FocusFlowAPI.Models
{
    [Table("sesiones_enfoque")]
    public class SesionEnfoque
    {
        [Key]
        [Column("id_sesion")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdSesion { get; set; }

        [Required]
        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Required]
        [Column("duracion_minutos")]
        public int DuracionMinutos { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        [Column("tipo")]
        public required string Tipo { get; set; }
    }
}
