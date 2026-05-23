using System.Text.Json.Serialization;

namespace FocusFlowAPI.DTOs
{
    public class PerfilUsuarioDto
    {
        public int IdPerfil { get; set; }
        public Guid IdUsuario { get; set; }
        public required string Nombre { get; set; }
        public string? Rol { get; set; }
        public int? Edad { get; set; }
        public string? Ocupacion { get; set; }

        [JsonPropertyName("objetivo_principal")]
        public string? ObjetivoPrincipal { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
