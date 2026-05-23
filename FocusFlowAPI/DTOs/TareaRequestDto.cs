using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using FocusFlowAPI.Serialization;
namespace FocusFlowAPI.DTOs
{
    public class TareaRequestDto
    {
        [Required(ErrorMessage = "El título es obligatorio.")]
        public required string Titulo { get; set; }
        public string? Descripcion { get; set; }
        [Required(ErrorMessage = "La prioridad es obligatoria.")]
        public required string Prioridad { get; set; }
        [Required(ErrorMessage = "El nivel de esfuerzo es obligatorio.")]
        public required string NivelEsfuerzo { get; set; }
        [Required(ErrorMessage = "El estado es obligatorio.")]
        public required string Estado { get; set; }
        [JsonConverter(typeof(UtcNullableDateTimeConverter))]
        public DateTime? FechaLimite { get; set; }
    }
}