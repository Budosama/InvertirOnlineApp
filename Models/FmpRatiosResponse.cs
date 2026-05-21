namespace InvertirOnlineApp.Models
{
    public class FmpRatiosResponse
    {
        public decimal? priceEarningsRatio { get; set; }
        public decimal? priceToBookRatio { get; set; }
        public decimal? returnOnEquity { get; set; }
        public decimal? returnOnAssets { get; set; }
        public decimal? debtEquityRatio { get; set; }
        public decimal? currentRatio { get; set; }
        public decimal? quickRatio { get; set; }
        public decimal? dividendYield { get; set; }
        public decimal? payoutRatio { get; set; }
        public decimal? operatingProfitMargin { get; set; }
        public decimal? netProfitMargin { get; set; }
    }
}