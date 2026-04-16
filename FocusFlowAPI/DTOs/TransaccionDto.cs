namespace FocusFlowAPI.DTOs
{
    public class TransaccionDto
    {
        public int IdTransaccion { get; set; }
        public required decimal Monto { get; set; }
        public required string Tipo { get; set; }
        public string? Categoria { get; set; }
        public string? Descripcion { get; set; }
        public DateTime Fecha { get; set; }
    }
}
