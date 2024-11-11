namespace InvertirOnlineApp.Models
{
    public class Titulo
    {
        public string simbolo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string pais { get; set; } = string.Empty;
        public string mercado { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string plazo { get; set; } = string.Empty;
        public string moneda { get; set; } = string.Empty;
    }

    public class Activo
    {
        public decimal cantidad { get; set; }
        public decimal comprometido { get; set; }
        public decimal puntosVariacion { get; set; }
        public decimal variacionDiaria { get; set; }
        public decimal ultimoPrecio { get; set; }
        public decimal ppc { get; set; }
        public decimal gananciaPorcentaje { get; set; }
        public decimal gananciaDinero { get; set; }
        public decimal valorizado { get; set; }
        public Titulo titulo { get; set; } = new Titulo();
        public object? parking { get; set; }
        public DateTime? fechaPrimerCompra { get; set; }
        public decimal? sumatoriaMontoCompras { get; set; }
        public int DiasDesdePrimerCompra()
        {
            if (fechaPrimerCompra.HasValue)
            {
                return (DateTime.Now - fechaPrimerCompra.Value).Days;
            }
            return 0;
        }
    }

    public class PortafolioResponse
    {
        public string pais { get; set; } = string.Empty;
        public List<Activo> activos { get; set; } = new List<Activo>();
    }

}
