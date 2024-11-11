namespace InvertirOnlineApp.Models
{
    public class Operacion
    {
        public int? numero { get; set; }
        public DateTime? fechaOrden { get; set; }
        public string? tipo { get; set; }
        public string? estado { get; set; }
        public string? mercado { get; set; }
        public string? simbolo { get; set; }
        public decimal? cantidad { get; set; }
        public decimal? monto { get; set; } 
        public string? modalidad { get; set; }
        public decimal? precio { get; set; } 
        public DateTime? fechaOperada { get; set; }
        public decimal? cantidadOperada { get; set; } 
        public decimal? precioOperado { get; set; } 
        public decimal? montoOperado { get; set; }
        public string? plazo { get; set; }
    }

}
