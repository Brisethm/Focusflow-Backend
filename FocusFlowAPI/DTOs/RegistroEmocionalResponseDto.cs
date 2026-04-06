namespace FocusFlowAPI.DTOs
{
    public class RegistroEmocionalResponseDto
    {
        public int IdRegistro { get; set; }
        public Guid IdUsuario { get; set; }
        public string EstadoAnimo { get; set; } = string.Empty;
        public int? NivelEnergia { get; set; }
        public string? NotaOpcional { get; set; }
        public DateTime Fecha { get; set; }
    }
}
