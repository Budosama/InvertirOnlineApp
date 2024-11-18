namespace InvertirOnlineApp.Models
{
    public class VariableEconomica
    {
        public int? idVariable { get; set; }
        public int? cdSerie { get; set; }
        public string? descripcion { get; set; }
        public DateTime? fecha { get; set; }
        public decimal? valor { get; set; }
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
