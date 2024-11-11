namespace InvertirOnlineApp.Models
{
    public class BancoInfo
    {
        public string entidad { get; set; } = string.Empty;
        public string logo { get; set; } = string.Empty;
        public double? tnaClientes { get; set; }
        public double? tnaNoClientes { get; set; }
        public string enlace { get; set; } = string.Empty;
    }

}
