namespace InvertirOnlineApp.Models
{
    public class FmpProfileResponse
    {
        public string symbol { get; set; }
        public string companyName { get; set; }
        public string sector { get; set; }
        public string industry { get; set; }
        public decimal? price { get; set; }
        public decimal? mktCap { get; set; }
    }
}
