using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FocusFlowAPI.Models
{
    [Table("transacciones")]
    public class Transaccion
    {
        [Key]
        [Column("id_transaccion")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdTransaccion { get; set; }

        [Required]
        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Required]
        [Column("monto")]
        public decimal Monto { get; set; }

        [MaxLength(50)]
        [Column("tipo")]
        public required string Tipo { get; set; }

        [MaxLength(100)]
        [Column("categoria")]
        public string? Categoria { get; set; }

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
