using System;
using System.ComponentModel.DataAnnotations;

namespace FocusFlowAPI.Models
{
    public class RegistroEmocional
    {
        [Key]
        public int IdRegistro { get; set; }

        [Required]
        public Guid IdUsuario { get; set; }

        [MaxLength(100)]
        public required string  EstadoAnimo { get; set; }

        public int? NivelEnergia { get; set; }

        public string ? NotaOpcional { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
