namespace FocusFlowAPI.DTOs
{
    public class RegistroEmocionalDto
    {
        public required string EstadoAnimo { get; set; }
        public int? NivelEnergia { get; set; }
        public string ? NotaOpcional { get; set; }
    }
}
