using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FocusFlowAPI.Models
{
    [Table("planes_personalizados")]
    public class PlanPersonalizado
    {
        [Key]
        [Column("id_plan")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdPlan { get; set; }

        [Required]
        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Column("id_cuestionario")]
        public int? IdCuestionario { get; set; }

        [Column("hora_descanso")]
        public TimeOnly? HoraDescanso { get; set; }

        [Column("enfoque_diario")]
        public int? EnfoqueDiario { get; set; }

        [Column("pausas_diarias")]
        public int? PausasDiarias { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
