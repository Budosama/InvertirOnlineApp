namespace InvertirOnlineApp.Models
{
    public class EmpresaTecnica
    {
        public string Symbol { get; set; } = "";

        public string CompanyName { get; set; } = "";

        public string Sector { get; set; } = "";

        public decimal? Price { get; set; }

        // TREND
        public decimal? SMA20 { get; set; }
        public decimal? SMA50 { get; set; }
        public decimal? SMA200 { get; set; }

        public decimal? EMA20 { get; set; }

        public string Trend { get; set; } = "";

        // MOMENTUM
        public decimal? RSI { get; set; }

        public decimal? MACD { get; set; }

        public decimal? MACDSignal { get; set; }

        // VOLATILITY
        public decimal? Volatility { get; set; }

        // SCORE
        public int TechnicalScore { get; set; }

        public string Signal { get; set; } = "";

        public string Recommendation { get; set; } = "";

        public List<string> Reasons { get; set; }
            = new();
    }
}
