using System;
using System.ComponentModel.DataAnnotations;

namespace FocusFlowAPI.Models
{
    public class Recordatorio
    {
        [Key]
        public int IdRecordatorio { get; set; }

        [Required]
        public Guid IdUsuario { get; set; }

        [Required]
        public string ? Mensaje { get; set; }

        [Required]
        public DateTime FechaHora { get; set; }

        [MaxLength(50)]
        public string ? Tipo { get; set; }

        public bool Activo { get; set; } = true;
    }
}
