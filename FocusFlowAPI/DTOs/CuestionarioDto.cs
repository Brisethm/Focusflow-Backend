using System;
using System.Collections.Generic;

namespace FocusFlowAPI.DTOs
{
    public class CuestionarioDto
    {
        public int? PuntajeTotal { get; set; }
        public bool? Completado { get; set; }
        public int? NivelEstres { get; set; }
        public int? NivelEnergia { get; set; }
        public int? NivelOrganizacion { get; set; }
        public int? NivelProcrastinacion { get; set; }
        public string? Perfil { get; set; }

        public List<RespuestaDto>? Respuestas { get; set; }
    }

    public class RespuestaDto
    {
        public required string Pregunta { get; set; }
        public required string Valor { get; set; }

        public string? Categoria { get; set; }
        public int? Puntaje { get; set; }
    }
}