namespace InvertirOnlineApp.Models
{
    public class VariableEconomica
    {
        public int? idVariable { get; set; }
        public string? descripcion { get; set; }
        public string? categoria { get; set; }
        public string? tipoSerie { get; set; }
        public string? periodicidad { get; set; }
        public string? unidadExpresion { get; set; }
        public string? moneda { get; set; }
        public DateTime? primerFechaInformada { get; set; }
        public DateTime? ultFechaInformada { get; set; }
        public decimal? ultValorInformado { get; set; }
    }

    public class ApiResponse
    {
        public int? status { get; set; }
        public List<VariableEconomica> results { get; set; } = new List<VariableEconomica>();
    }

    public class RiesgoPais
    {
        public decimal? valor { get; set; }
        public DateTime? fecha { get; set; }
    }
}
