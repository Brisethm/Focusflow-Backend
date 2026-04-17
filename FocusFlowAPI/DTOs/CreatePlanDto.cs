using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using FocusFlowAPI.Serialization;

namespace FocusFlowAPI.DTOs
{
    public class CreatePlanDto
    {
        [Required(ErrorMessage = "La hora de descanso es obligatoria.")]
        [JsonConverter(typeof(TimeOnlyJsonConverter))]
        public TimeOnly? HoraDescanso { get; set; }

        [Required(ErrorMessage = "El enfoque diario es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El enfoque diario debe ser mayor a 0.")]
        public int? EnfoqueDiario { get; set; }

        [Required(ErrorMessage = "Las pausas diarias son obligatorias.")]
        [Range(1, int.MaxValue, ErrorMessage = "Las pausas diarias deben ser mayores a 0.")]
        public int? PausasDiarias { get; set; }

        public int? IdCuestionario { get; set; }
    }
}
