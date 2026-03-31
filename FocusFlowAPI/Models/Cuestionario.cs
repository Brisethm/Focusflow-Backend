using System;
using System.ComponentModel.DataAnnotations;

namespace FocusFlowAPI.Models
{
    public class Cuestionario
    {
        [Key]
        public int IdCuestionario { get; set; }

        [Required]
        public Guid IdUsuario { get; set; }

        public int? PuntajeTotal { get; set; }

        public bool Completado { get; set; } = false;

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public ICollection<RespuestaCuestionario>? Respuestas { get; set; }
    }
}
