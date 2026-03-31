using System;
using System.ComponentModel.DataAnnotations;

namespace FocusFlowAPI.Models
{
    public class Tarea
    {
        [Key]
        public int IdTarea { get; set; }

        [Required]
        public Guid IdUsuario { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Titulo { get; set; }

        [MaxLength(50)]
        public required string Prioridad { get; set; }

        [MaxLength(50)]
        public required string NivelEsfuerzo { get; set; }

        [MaxLength(50)]
        public required string Estado { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaLimite { get; set; }
    }
}
