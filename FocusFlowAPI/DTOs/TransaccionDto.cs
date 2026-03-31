namespace FocusFlowAPI.DTOs
{
    public class TransaccionDto
    {
        public required decimal Monto { get; set; }
        public required string Tipo { get; set; }
        public string ? Categoria { get; set; }
        public string ? Descripcion { get; set; }
    }
}
