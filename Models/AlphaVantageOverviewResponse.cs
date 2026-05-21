namespace InvertirOnlineApp.Models
{
    public class AlphaVantageOverviewResponse
    {
        public string Symbol { get; set; }
        public string Name { get; set; }

        public string Sector { get; set; }
        public string Industry { get; set; }

        public string MarketCapitalization { get; set; }

        public string PERatio { get; set; }

        public string PEGRatio { get; set; }

        public string PriceToBookRatio { get; set; }

        public string ReturnOnEquityTTM { get; set; }

        public string ReturnOnAssetsTTM { get; set; }

        public string ProfitMargin { get; set; }

        public string OperatingMarginTTM { get; set; }

        public string QuarterlyRevenueGrowthYOY { get; set; }

        public string QuarterlyEarningsGrowthYOY { get; set; }

        public string DividendYield { get; set; }

        public string DebtToEquity { get; set; }

        public string Beta { get; set; }
    }
}