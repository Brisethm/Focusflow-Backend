using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FocusFlowAPI.Models
{
    [Table("perfil_usuario")]
    public class PerfilUsuario
    {
        [Key]
        [Column("id_perfil")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int IdPerfil { get; set; }

        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Column("nombre")]
        public required string Nombre { get; set; }


        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }

}
