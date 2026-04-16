using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace FocusFlowAPI.Models
{
    [Table("cuestionarios")]
    public class Cuestionario
    {
        public Cuestionario()
        {
            Respuestas = new List<RespuestaCuestionario>();
        }

        [Key]
        [Column("id_cuestionario")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdCuestionario { get; set; }

        [Required]
        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Column("puntaje_total")]
        public int? PuntajeTotal { get; set; }

        [Column("completado")]
        public bool Completado { get; set; } = false;

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Column("nivel_estres")]
        public int? NivelEstres { get; set; }

        [Column("nivel_energia")]
        public int? NivelEnergia { get; set; }

        [Column("nivel_organizacion")]
        public int? NivelOrganizacion { get; set; }

        [Column("nivel_procrastinacion")]
        public int? NivelProcrastinacion { get; set; }

        [MaxLength(50)]
        [Column("perfil")]
        public string? Perfil { get; set; }

        public ICollection<RespuestaCuestionario> Respuestas { get; set; } = new List<RespuestaCuestionario>();
    }
}