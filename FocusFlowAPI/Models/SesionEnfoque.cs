using System;
using System.ComponentModel.DataAnnotations;

namespace FocusFlowAPI.Models
{
    public class SesionEnfoque
    {
        [Key]
        public int IdSesion { get; set; }

        [Required]
        public Guid IdUsuario { get; set; }

        [Required]
        public int DuracionMinutos { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public required string Tipo { get; set; }
    }
}
