namespace InvertirOnlineApp.Models
{
    public class CotizacionActivoResponse
    {
        public List<TituloV2> titulos { get; set; } = new List<TituloV2>();
    }

    public class TituloV2
    {
        public string simbolo { get; set; } = string.Empty;
        public Puntas puntas { get; set; } = new Puntas();
        public decimal? ultimoPrecio { get; set; }
        public decimal? variacionPorcentual { get; set; }
        public decimal? apertura { get; set; }
        public decimal? maximo { get; set; }
        public decimal? minimo { get; set; }
        public decimal? ultimoCierre { get; set; }
        public decimal? volumen { get; set; }
        public decimal? cantidadOperaciones { get; set; }
        public DateTime? fecha { get; set; }
        public string tipoOpcion { get; set; } = string.Empty;
        public decimal? precioEjercicio { get; set; }
        public string fechaVencimiento { get; set; } = string.Empty;
        public string mercado { get; set; } = string.Empty;
        public string moneda { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string plazo { get; set; } = string.Empty;
        public decimal? laminaMinima { get; set; }
        public decimal? lote { get; set; }
    }

    public class Puntas
    {
        public decimal? cantidadCompra { get; set; }
        public decimal? precioCompra { get; set; }
        public decimal? precioVenta { get; set; }
        public decimal? cantidadVenta { get; set; }
    }

}