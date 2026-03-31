using System.Collections.Generic;

namespace FocusFlowAPI.DTOs
{
    public class CuestionarioDto
    {
        public int? PuntajeTotal { get; set; }
        public bool? Completado { get; set; }

        public List<RespuestaDto>? Respuestas { get; set; }
    }

    public class RespuestaDto
    {
        public required string Pregunta { get; set; }
        public required string Valor { get; set; }
    }
}
