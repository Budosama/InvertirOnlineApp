// =============================================
// Services/FundamentalAnalysisService.cs
// =============================================

using System.Globalization;
using System.Text.Json;
using InvertirOnlineApp.Models;

namespace InvertirOnlineApp.Services
{
    public class FundamentalAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public FundamentalAnalysisService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        private string ApiKey =>
            _configuration["AlphaVantage:ApiKey"]!;

        public async Task<List<EmpresaFundamental>> GetEmpresasAsync(string? symbol = null)
        {
            List<string> symbols;

            if (!string.IsNullOrWhiteSpace(symbol))
            {
                symbols = new List<string>
                {
                    symbol
                };
            }
            else
            {
                symbols = new List<string>
                {
                    "AAPL",
                    "MSFT",
                    "GOOGL",
                    "AMZN",
                    "META",
                    "NVDA",
                    "AMD",
                    "ASML",
                    "TSM",
                    "COST",
                    "WMT",
                    "KO",
                    "PEP",
                    "MCD",
                    "BRK-B",
                    "JPM",
                    "V",
                    "MA",
                    "JNJ",
                    "LLY",
                    "CAT",
                    "XOM",
                    "SPY",
                    "QQQ"
                };
            }

            var empresas = new List<EmpresaFundamental>();

            foreach (var s in symbols)
            {
                var empresa =
                    await GetEmpresaAsync(s);

                if (empresa != null)
                {
                    empresa.FundamentalScore =
                        CalcularScore(empresa);

                    empresa.Recommendation =
                        GetRecommendation(
                            empresa.FundamentalScore);

                    empresas.Add(empresa);
                }
            }

            return empresas
                .OrderByDescending(x => x.FundamentalScore)
                .ToList();
        }

        public async Task<EmpresaFundamental?> GetEmpresaAsync(string symbol)
        {
            try
            {
                var url =
                    $"https://www.alphavantage.co/query?function=OVERVIEW&symbol={symbol}&apikey={ApiKey}";

                Console.WriteLine(url);

                var response =
                    await _httpClient.GetStringAsync(url);

                Console.WriteLine(response);

                var data =
                    JsonSerializer.Deserialize<AlphaVantageOverviewResponse>(
                        response,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (data == null ||
                    string.IsNullOrEmpty(data.Symbol))
                {
                    return null;
                }

                return new EmpresaFundamental
                {
                    Symbol = data.Symbol,
                    CompanyName = data.Name,

                    Sector = data.Sector,
                    Industry = data.Industry,

                    MarketCap =
                        ParseDecimal(data.MarketCapitalization),

                    PERatio =
                        ParseDecimal(data.PERatio),

                    PegRatio =
                        ParseDecimal(data.PEGRatio),

                    PriceToBook =
                        ParseDecimal(data.PriceToBookRatio),

                    ROE =
                        ParseDecimal(data.ReturnOnEquityTTM) * 100,

                    ROA =
                        ParseDecimal(data.ReturnOnAssetsTTM) * 100,

                    NetMargin =
                        ParseDecimal(data.ProfitMargin) * 100,

                    OperatingMargin =
                        ParseDecimal(data.OperatingMarginTTM) * 100,

                    RevenueGrowth =
                        ParseDecimal(data.QuarterlyRevenueGrowthYOY) * 100,

                    EPSGrowth =
                        ParseDecimal(data.QuarterlyEarningsGrowthYOY) * 100,

                    DividendYield =
                        ParseDecimal(data.DividendYield) * 100,

                    DebtToEquity =
                        ParseDecimal(data.DebtToEquity)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                return null;
            }
        }

        private decimal? ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (value == "None")
                return null;

            if (decimal.TryParse(
                value,
                CultureInfo.InvariantCulture,
                out decimal result))
            {
                return result;
            }

            return null;
        }

        private int CalcularScore(EmpresaFundamental e)
        {
            double score = 0;

            // =============================================
            // SECTOR DETECTION
            // =============================================

            bool isTech =
                e.Sector?.Contains("Technology",
                    StringComparison.OrdinalIgnoreCase) == true;

            bool isFinancial =
                e.Sector?.Contains("Financial",
                    StringComparison.OrdinalIgnoreCase) == true;

            bool isHealthcare =
                e.Sector?.Contains("Healthcare",
                    StringComparison.OrdinalIgnoreCase) == true;

            bool isConsumer =
                e.Sector?.Contains("Consumer",
                    StringComparison.OrdinalIgnoreCase) == true;

            bool isIndustrial =
                e.Sector?.Contains("Industrial",
                    StringComparison.OrdinalIgnoreCase) == true;

            bool isETF =
                e.Industry?.Contains("ETF",
                    StringComparison.OrdinalIgnoreCase) == true;

            // ETFs
            if (isETF)
            {
                return 75;
            }

            // =============================================
            // PROFITABILITY (25)
            // =============================================

            // ROE
            if (isFinancial)
            {
                if (e.ROE >= 18)
                    score += 12;
                else if (e.ROE >= 14)
                    score += 8;
                else if (e.ROE >= 10)
                    score += 4;
            }
            else
            {
                if (e.ROE >= 20)
                    score += 10;
                else if (e.ROE >= 15)
                    score += 7;
                else if (e.ROE >= 10)
                    score += 4;
            }

            // Net Margin
            if (isTech || isHealthcare)
            {
                if (e.NetMargin >= 25)
                    score += 10;
                else if (e.NetMargin >= 18)
                    score += 7;
                else if (e.NetMargin >= 10)
                    score += 4;
            }
            else
            {
                if (e.NetMargin >= 15)
                    score += 10;
                else if (e.NetMargin >= 10)
                    score += 7;
                else if (e.NetMargin >= 5)
                    score += 4;
            }

            // Operating Margin
            if (e.OperatingMargin >= 25)
                score += 5;
            else if (e.OperatingMargin >= 15)
                score += 3;

            // =============================================
            // GROWTH (30)
            // =============================================

            // EPS Growth
            if (isTech)
            {
                if (e.EPSGrowth >= 30)
                    score += 15;
                else if (e.EPSGrowth >= 20)
                    score += 11;
                else if (e.EPSGrowth >= 10)
                    score += 6;
            }
            else
            {
                if (e.EPSGrowth >= 20)
                    score += 15;
                else if (e.EPSGrowth >= 12)
                    score += 10;
                else if (e.EPSGrowth >= 5)
                    score += 5;
            }

            // Revenue Growth
            if (isTech)
            {
                if (e.RevenueGrowth >= 20)
                    score += 15;
                else if (e.RevenueGrowth >= 12)
                    score += 10;
                else if (e.RevenueGrowth >= 7)
                    score += 5;
            }
            else
            {
                if (e.RevenueGrowth >= 12)
                    score += 10;
                else if (e.RevenueGrowth >= 7)
                    score += 7;
                else if (e.RevenueGrowth >= 3)
                    score += 4;
            }

            // =============================================
            // VALUATION (20)
            // =============================================

            // PEG Ratio (MUY importante)
            if (e.PegRatio > 0)
            {
                if (e.PegRatio < 1)
                    score += 12;
                else if (e.PegRatio <= 1.5m)
                    score += 9;
                else if (e.PegRatio <= 2)
                    score += 6;
                else if (e.PegRatio <= 3)
                    score += 3;
            }

            // P/E
            if (isTech)
            {
                if (e.PERatio > 0 && e.PERatio < 30)
                    score += 5;
                else if (e.PERatio < 45)
                    score += 3;
            }
            else if (isFinancial)
            {
                if (e.PERatio > 0 && e.PERatio < 15)
                    score += 8;
                else if (e.PERatio < 20)
                    score += 4;
            }
            else
            {
                if (e.PERatio > 0 && e.PERatio < 20)
                    score += 8;
                else if (e.PERatio < 30)
                    score += 4;
            }

            // P/B
            if (isFinancial)
            {
                if (e.PriceToBook < 1.5m)
                    score += 5;
                else if (e.PriceToBook < 2.5m)
                    score += 3;
            }
            else
            {
                if (e.PriceToBook < 3)
                    score += 5;
                else if (e.PriceToBook < 6)
                    score += 2;
            }

            // =============================================
            // FINANCIAL HEALTH (15)
            // =============================================

            if (!isFinancial)
            {
                // Debt
                if (e.DebtToEquity < 0.5m)
                    score += 8;
                else if (e.DebtToEquity < 1)
                    score += 5;
                else if (e.DebtToEquity < 2)
                    score += 2;

                // Liquidity
                if (e.CurrentRatio >= 2)
                    score += 7;
                else if (e.CurrentRatio >= 1.5m)
                    score += 5;
                else if (e.CurrentRatio >= 1)
                    score += 2;
            }
            else
            {
                // Bancos funcionan distinto
                score += 5;
            }

            // =============================================
            // QUALITY / STABILITY (10)
            // =============================================

            // Dividend
            if (e.DividendYield >= 1 &&
                e.DividendYield <= 5)
            {
                score += 2;
            }

            // Beta
            if (e.Beta <= 1.2m)
                score += 3;

            // Market Cap
            if (e.MarketCap >= 500_000_000_000)
                score += 5;
            else if (e.MarketCap >= 100_000_000_000)
                score += 4;
            else if (e.MarketCap >= 10_000_000_000)
                score += 2;

            // =============================================
            // QUALITY COMPOUNDER BONUS
            // =============================================

            if (e.ROE > 20 &&
                e.NetMargin > 20 &&
                e.EPSGrowth > 15 &&
                e.RevenueGrowth > 10)
            {
                score += 7;
            }

            // =============================================
            // MEGA CAP DOMINANCE BONUS
            // =============================================

            if (isTech &&
                e.MarketCap > 1_000_000_000_000 &&
                e.ROE > 20)
            {
                score += 3;
            }

            // =============================================
            // PENALTIES
            // =============================================

            // Value trap
            if (e.PERatio > 0 &&
                e.PERatio < 10 &&
                e.RevenueGrowth < 0)
            {
                score -= 10;
            }

            // Negative growth
            if (e.EPSGrowth < 0)
                score -= 8;

            if (e.RevenueGrowth < 0)
                score -= 6;

            // Excessive debt
            if (!isFinancial &&
                e.DebtToEquity > 3)
            {
                score -= 12;
            }

            // Expensive without growth
            if (e.PERatio > 50 &&
                e.EPSGrowth < 10)
            {
                score -= 10;
            }

            // Unprofitable
            if (e.NetMargin < 0)
            {
                score -= 15;
            }

            // =============================================
            // FINAL CAP
            // =============================================

            score =
                Math.Max(0,
                Math.Min(100, score));

            return (int)Math.Round(score);
        }

        private string GetRecommendation(int score)
        {
            if (score >= 85)
                return "🟢 ELITE";

            if (score >= 75)
                return "🟢 STRONG BUY";

            if (score >= 60)
                return "🟡 BUY";

            if (score >= 45)
                return "🟠 HOLD";

            if (score >= 30)
                return "🔴 WEAK";

            return "⚫ AVOID";
        }
    }
}