namespace InvertirOnlineApp.Models
{
    public class EmpresaFundamental
    {
        public string Symbol { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string Sector { get; set; } = "";
        public string Industry { get; set; } = "";

        // VALUATION
        public decimal? PERatio { get; set; }
        public decimal? PegRatio { get; set; }
        public decimal? PriceToBook { get; set; }
        public decimal? EVToEBITDA { get; set; }

        // PROFITABILITY
        public decimal? ROE { get; set; }
        public decimal? ROA { get; set; }
        public decimal? NetMargin { get; set; }
        public decimal? OperatingMargin { get; set; }

        // GROWTH
        public decimal? RevenueGrowth { get; set; }
        public decimal? EPSGrowth { get; set; }

        // FINANCIAL HEALTH
        public decimal? DebtToEquity { get; set; }
        public decimal? CurrentRatio { get; set; }
        public decimal? QuickRatio { get; set; }

        // DIVIDENDS
        public decimal? DividendYield { get; set; }
        public decimal? PayoutRatio { get; set; }

        // MARKET
        public decimal? MarketCap { get; set; }
        public decimal? Price { get; set; }

        // SCORE
        public int FundamentalScore { get; set; }
        public string Recommendation { get; set; } = "";

        public decimal? Beta { get; set; }

    }
}
