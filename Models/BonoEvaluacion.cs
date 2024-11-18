namespace InvertirOnlineApp.Models
{
    public class BonoEvaluacion
    {
        public decimal Rendimiento { get; set; }
        public decimal Inflacion { get; set; }
        public decimal TasaReferencia { get; set; }
        public decimal RiesgoPais { get; set; }
        public decimal Spread { get; set; }
        public string Rating { get; set; } = string.Empty;
        public bool EsAtractivo { get; set; }
    }

}