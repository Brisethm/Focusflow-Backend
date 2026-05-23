using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FocusFlowAPI.Models
{
    [Table("respuestas_cuestionarios")]
    public class RespuestaCuestionario
    {
        [Key]
        [Column("id_respuestas")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdRespuesta { get; set; }

        [Required]
        [Column("id_cuestionario")]
        public int IdCuestionario { get; set; }

        [Required]
        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Required]
        [Column("pregunta")]
        public string Pregunta { get; set; } = string.Empty;

        [Column("valor")]
        public string? Valor { get; set; }

        [Column("categoria")]
        public string? Categoria { get; set; }

        [Column("puntaje")]
        public int? Puntaje { get; set; }

        [ForeignKey("IdCuestionario")]
        public Cuestionario? Cuestionario { get; set; }
    }
}
