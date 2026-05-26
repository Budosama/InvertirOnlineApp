using System.Text.Json.Serialization;

namespace InvertirOnlineApp.Models
{
    public class AlphaVantageOverviewResponse
    {
        // =============================================
        // BASIC
        // =============================================

        public string Symbol { get; set; }

        public string AssetType { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Exchange { get; set; }

        public string Currency { get; set; }

        public string Country { get; set; }

        public string Sector { get; set; }

        public string Industry { get; set; }

        public string Address { get; set; }

        public string OfficialSite { get; set; }

        // =============================================
        // MARKET DATA
        // =============================================

        public string MarketCapitalization { get; set; }

        public string EBITDA { get; set; }

        public string RevenueTTM { get; set; }

        public string GrossProfitTTM { get; set; }

        public string RevenuePerShareTTM { get; set; }

        public string EPS { get; set; }

        public string DilutedEPSTTM { get; set; }

        // =============================================
        // VALUATION
        // =============================================

        public string PERatio { get; set; }

        public string TrailingPE { get; set; }

        public string ForwardPE { get; set; }

        public string PEGRatio { get; set; }

        public string BookValue { get; set; }

        public string PriceToBookRatio { get; set; }

        public string PriceToSalesRatioTTM { get; set; }

        public string EVToRevenue { get; set; }

        public string EVToEBITDA { get; set; }

        // =============================================
        // PROFITABILITY
        // =============================================

        public string ProfitMargin { get; set; }

        public string OperatingMarginTTM { get; set; }

        public string ReturnOnAssetsTTM { get; set; }

        public string ReturnOnEquityTTM { get; set; }

        // =============================================
        // GROWTH
        // =============================================

        public string QuarterlyRevenueGrowthYOY { get; set; }

        public string QuarterlyEarningsGrowthYOY { get; set; }

        // =============================================
        // DIVIDENDS
        // =============================================

        public string DividendPerShare { get; set; }

        public string DividendYield { get; set; }

        public string DividendDate { get; set; }

        public string ExDividendDate { get; set; }

        public string PayoutRatio { get; set; }

        // =============================================
        // FINANCIAL HEALTH
        // =============================================

        public string DebtToEquity { get; set; }

        public string CurrentRatio { get; set; }

        public string QuickRatio { get; set; }

        // =============================================
        // ANALYSTS
        // =============================================

        public string AnalystTargetPrice { get; set; }

        public string AnalystRatingStrongBuy { get; set; }

        public string AnalystRatingBuy { get; set; }

        public string AnalystRatingHold { get; set; }

        public string AnalystRatingSell { get; set; }

        public string AnalystRatingStrongSell { get; set; }

        // =============================================
        // STOCK INFO
        // =============================================

        public string Beta { get; set; }

        public string SharesOutstanding { get; set; }

        public string SharesFloat { get; set; }

        public string PercentInsiders { get; set; }

        public string PercentInstitutions { get; set; }

        // =============================================
        // MOVING AVERAGES / PRICE LEVELS
        // =============================================

        [JsonPropertyName("52WeekHigh")]
        public string FiftyTwoWeekHigh { get; set; }

        [JsonPropertyName("52WeekLow")]
        public string FiftyTwoWeekLow { get; set; }

        [JsonPropertyName("50DayMovingAverage")]
        public string MovingAverage50Day { get; set; }

        [JsonPropertyName("200DayMovingAverage")]
        public string MovingAverage200Day { get; set; }

        // =============================================
        // FISCAL
        // =============================================

        public string FiscalYearEnd { get; set; }

        public string LatestQuarter { get; set; }

        public string CIK { get; set; }
    }
}