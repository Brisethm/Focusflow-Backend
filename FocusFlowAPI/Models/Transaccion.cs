using System;
using System.ComponentModel.DataAnnotations;

namespace FocusFlowAPI.Models
{
    public class Transaccion
    {
        [Key]
        public int IdTransaccion { get; set; }

        [Required]
        public Guid IdUsuario { get; set; }

        [Required]
        public decimal Monto { get; set; }

        [MaxLength(50)]
        public required string Tipo { get; set; }

        [MaxLength(100)]
        public string ? Categoria { get; set; }

        public string ? Descripcion { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
