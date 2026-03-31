using System.ComponentModel.DataAnnotations;

namespace FocusFlowAPI.Models
{
    public class RespuestaCuestionario
    {
        [Key]
        public int IdRespuestas { get; set; }

        [Required]
        public int IdCuestionario { get; set; }

        [Required]
        public required string Pregunta { get; set; }

        public required string Valor { get; set; }

        public Cuestionario? Cuestionario { get; set; }
    }
}
