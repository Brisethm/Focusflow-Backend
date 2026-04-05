using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FocusFlowAPI.Models
{
    [Table("registros_emocionales")]
    public class RegistroEmocional
    {
        [Key]
        [Column("id_registro")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdRegistro { get; set; }

        [Required]
        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [MaxLength(100)]
        [Column("estado_animo")]
        public required string  EstadoAnimo { get; set; }

        [Column("nivel_energia")]
        public int? NivelEnergia { get; set; }

        [Column("nota_opcional")]
        public string ? NotaOpcional { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
