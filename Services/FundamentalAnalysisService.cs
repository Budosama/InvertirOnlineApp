// =============================================
// Services/FundamentalAnalysisService.cs
// =============================================

using InvertirOnlineApp.Models;
using RestSharp;
using System.Globalization;
using System.Text.Json;

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

        // =============================================
        // MAIN
        // =============================================

        public async Task<List<EmpresaFundamental>>
            GetEmpresasAsync(
                string? symbol = null,
                string? accessToken = null)
        {
            List<string> symbols = new();

            if (!string.IsNullOrWhiteSpace(symbol))
            {
                symbols.Add(symbol);
            }
            else
            {
                // =============================================
                // PORTFOLIO
                // =============================================

                if (!string.IsNullOrEmpty(accessToken))
                {
                    try
                    {
                        var client =
                            new RestClient(
                                "https://api.invertironline.com");

                        var request =
                            new RestRequest(
                                "/api/v2/portafolio/argentina",
                                Method.Get);

                        request.AddHeader(
                            "Authorization",
                            $"Bearer {accessToken}");

                        var response =
                            await client.ExecuteAsync(request);

                        if (response.IsSuccessful &&
                            !string.IsNullOrEmpty(response.Content))
                        {
                            var portfolio =
                                JsonSerializer.Deserialize
                                    <PortafolioResponse>(
                                        response.Content);

                            if (portfolio?.activos != null)
                            {
                                symbols =
                                    portfolio.activos
                                        .Select(x =>
                                            x.titulo.simbolo)
                                        .Distinct()
                                        .ToList();
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                // =============================================
                // DEFAULT WATCHLIST
                // =============================================

                if (!symbols.Any())
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
                        "PLTR",
                        "NU",
                        "MELI",
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
            }

            var empresas =
                new List<EmpresaFundamental>();

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

                // AlphaVantage free tier
                await Task.Delay(12000);
            }

            return empresas
                .OrderByDescending(x =>
                    x.FundamentalScore)
                .ToList();
        }

        // =============================================
        // GET COMPANY
        // =============================================

        public async Task<EmpresaFundamental?>
            GetEmpresaAsync(string symbol)
        {
            try
            {
                var url =
                    $"https://www.alphavantage.co/query" +
                    $"?function=OVERVIEW" +
                    $"&symbol={symbol}" +
                    $"&apikey={ApiKey}";

                var response =
                    await _httpClient
                        .GetStringAsync(url);

                var data =
                    JsonSerializer.Deserialize
                        <AlphaVantageOverviewResponse>(
                            response,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                if (data == null ||
                    string.IsNullOrWhiteSpace(
                        data.Symbol))
                {
                    return null;
                }

                return new EmpresaFundamental
                {
                    Symbol = data.Symbol,

                    CompanyName =
                        data.Name ?? "",

                    Sector =
                        data.Sector ?? "",

                    Industry =
                        data.Industry ?? "",

                    // =============================================
                    // VALUATION
                    // =============================================

                    PERatio =
                        ParseDecimal(data.PERatio),

                    PegRatio =
                        ParseDecimal(data.PEGRatio),

                    PriceToBook =
                        ParseDecimal(
                            data.PriceToBookRatio),

                    EVToEBITDA =
                        ParseDecimal(
                            data.EVToEBITDA),

                    // =============================================
                    // PROFITABILITY
                    // =============================================

                    ROE =
                        Percent(
                            data.ReturnOnEquityTTM),

                    ROA =
                        Percent(
                            data.ReturnOnAssetsTTM),

                    NetMargin =
                        Percent(
                            data.ProfitMargin),

                    OperatingMargin =
                        Percent(
                            data.OperatingMarginTTM),

                    // =============================================
                    // GROWTH
                    // =============================================

                    RevenueGrowth =
                        Percent(
                            data.QuarterlyRevenueGrowthYOY),

                    EPSGrowth =
                        Percent(
                            data.QuarterlyEarningsGrowthYOY),

                    // =============================================
                    // FINANCIAL HEALTH
                    // =============================================

                    DebtToEquity =
                        ParseDecimal(
                            data.DebtToEquity),

                    CurrentRatio =
                        ParseDecimal(
                            data.CurrentRatio),

                    QuickRatio =
                        ParseDecimal(
                            data.QuickRatio),

                    // =============================================
                    // DIVIDENDS
                    // =============================================

                    DividendYield =
                        Percent(
                            data.DividendYield),

                    PayoutRatio =
                        Percent(
                            data.PayoutRatio),

                    // =============================================
                    // MARKET
                    // =============================================

                    MarketCap =
                        ParseDecimal(
                            data.MarketCapitalization),

                    Price =
                        ParseDecimal(
                            data.AnalystTargetPrice),

                    Beta =
                        ParseDecimal(
                            data.Beta),

                    // =============================================
                    // EXTRA
                    // =============================================

                    GrossProfitTTM =
                        data.GrossProfitTTM,

                    RevenueTTM =
                        data.RevenueTTM,

                    EBITDA =
                        data.EBITDA,

                    PriceToSalesRatioTTM =
                        data.PriceToSalesRatioTTM,

                    AnalystTargetPrice =
                        data.AnalystTargetPrice,

                    AnalystRatingStrongBuy =
                        data.AnalystRatingStrongBuy,

                    AnalystRatingBuy =
                        data.AnalystRatingBuy,

                    AnalystRatingHold =
                        data.AnalystRatingHold,

                    AnalystRatingSell =
                        data.AnalystRatingSell,

                    AnalystRatingStrongSell =
                        data.AnalystRatingStrongSell
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return null;
            }
        }

        // =============================================
        // SCORE ENGINE
        // =============================================

        private int CalcularScore(
            EmpresaFundamental e)
        {
            double score = 50;

            bool isTech =
                Contains(e.Sector, "Technology") ||
                Contains(e.Industry, "Software") ||
                Contains(e.Industry, "Semiconductor");

            bool isFinancial =
                Contains(e.Sector, "Financial");

            bool isHealthcare =
                Contains(e.Sector, "Healthcare");

            bool isETF =
                Contains(e.Industry, "ETF");

            // =============================================
            // ETFs
            // =============================================

            if (isETF)
                return 75;

            // =============================================
            // PROFITABILITY
            // =============================================

            if (e.ROE >= 25)
                score += 12;
            else if (e.ROE >= 18)
                score += 8;
            else if (e.ROE >= 12)
                score += 4;
            else if (e.ROE < 5)
                score -= 8;

            if (e.NetMargin >= 25)
                score += 12;
            else if (e.NetMargin >= 15)
                score += 8;
            else if (e.NetMargin >= 8)
                score += 4;
            else if (e.NetMargin < 0)
                score -= 15;

            if (e.OperatingMargin >= 25)
                score += 8;
            else if (e.OperatingMargin >= 15)
                score += 4;
            else if (e.OperatingMargin < 5)
                score -= 5;

            // =============================================
            // GROWTH
            // =============================================

            if (isTech)
            {
                if (e.RevenueGrowth >= 25)
                    score += 12;
                else if (e.RevenueGrowth >= 15)
                    score += 8;
                else if (e.RevenueGrowth >= 8)
                    score += 4;
            }
            else
            {
                if (e.RevenueGrowth >= 12)
                    score += 10;
                else if (e.RevenueGrowth >= 6)
                    score += 5;
            }

            if (e.EPSGrowth >= 30)
                score += 15;
            else if (e.EPSGrowth >= 15)
                score += 10;
            else if (e.EPSGrowth >= 5)
                score += 5;
            else if (e.EPSGrowth < 0)
                score -= 10;

            // =============================================
            // VALUATION
            // =============================================

            if (e.PegRatio > 0)
            {
                if (e.PegRatio < 1)
                    score += 12;
                else if (e.PegRatio <= 1.5m)
                    score += 8;
                else if (e.PegRatio <= 2.5m)
                    score += 4;
                else if (e.PegRatio > 4)
                    score -= 8;
            }

            if (isTech)
            {
                if (e.PERatio > 0 &&
                    e.PERatio <= 35)
                {
                    score += 6;
                }
                else if (e.PERatio > 60)
                {
                    score -= 6;
                }
            }
            else if (isFinancial)
            {
                if (e.PERatio > 0 &&
                    e.PERatio <= 15)
                {
                    score += 8;
                }
            }
            else
            {
                if (e.PERatio > 0 &&
                    e.PERatio <= 22)
                {
                    score += 6;
                }
            }

            // =============================================
            // FINANCIAL HEALTH
            // =============================================

            if (!isFinancial)
            {
                if (e.DebtToEquity < 0.5m)
                    score += 8;
                else if (e.DebtToEquity < 1)
                    score += 5;
                else if (e.DebtToEquity > 3)
                    score -= 10;

                if (e.CurrentRatio >= 2)
                    score += 6;
                else if (e.CurrentRatio >= 1.2m)
                    score += 3;
                else if (e.CurrentRatio < 1)
                    score -= 5;
            }

            // =============================================
            // STABILITY
            // =============================================

            if (e.Beta <= 1.2m)
                score += 4;
            else if (e.Beta > 2)
                score -= 5;

            if (e.DividendYield >= 1 &&
                e.DividendYield <= 5)
            {
                score += 3;
            }

            // =============================================
            // MARKET CAP
            // =============================================

            if (e.MarketCap >= 1_000_000_000_000)
                score += 8;
            else if (e.MarketCap >= 100_000_000_000)
                score += 5;
            else if (e.MarketCap < 2_000_000_000)
                score -= 5;

            // =============================================
            // QUALITY COMPOUNDER
            // =============================================

            if (e.ROE > 20 &&
                e.NetMargin > 15 &&
                e.EPSGrowth > 15 &&
                e.RevenueGrowth > 10)
            {
                score += 10;
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

            // Expensive no-growth
            if (e.PERatio > 60 &&
                e.EPSGrowth < 10)
            {
                score -= 12;
            }

            // Weak business
            if (e.ROE < 5 &&
                e.NetMargin < 5)
            {
                score -= 10;
            }

            // =============================================
            // ANALYST CONSENSUS
            // =============================================

            int strongBuy =
                ParseInt(
                    e.AnalystRatingStrongBuy);

            int buy =
                ParseInt(
                    e.AnalystRatingBuy);

            int hold =
                ParseInt(
                    e.AnalystRatingHold);

            int sell =
                ParseInt(
                    e.AnalystRatingSell);

            int strongSell =
                ParseInt(
                    e.AnalystRatingStrongSell);

            int total =
                strongBuy + buy +
                hold + sell +
                strongSell;

            if (total > 0)
            {
                var bullish =
                    strongBuy + buy;

                var bearish =
                    sell + strongSell;

                var ratio =
                    (decimal)bullish / total;

                if (ratio >= 0.7m)
                    score += 5;
                else if (ratio <= 0.3m)
                    score -= 5;

                if (bearish >= bullish)
                    score -= 5;
            }

            // =============================================
            // FINAL
            // =============================================

            score =
                Math.Max(
                    0,
                    Math.Min(100, score));

            return (int)Math.Round(score);
        }

        // =============================================
        // RECOMMENDATION
        // =============================================

        private string GetRecommendation(
            int score)
        {
            if (score >= 90)
                return "🟢 ELITE";

            if (score >= 80)
                return "🟢 STRONG BUY";

            if (score >= 65)
                return "🟡 BUY";

            if (score >= 50)
                return "🟠 HOLD";

            if (score >= 35)
                return "🔴 WEAK";

            return "⚫ AVOID";
        }

        // =============================================
        // HELPERS
        // =============================================

        private decimal? ParseDecimal(
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (value == "None")
                return null;

            if (decimal.TryParse(
                value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out decimal result))
            {
                return result;
            }

            return null;
        }

        private decimal? Percent(
            string? value)
        {
            var parsed =
                ParseDecimal(value);

            if (!parsed.HasValue)
                return null;

            return parsed.Value * 100;
        }

        private bool Contains(
            string? source,
            string value)
        {
            return source?
                .Contains(
                    value,
                    StringComparison.OrdinalIgnoreCase)
                == true;
        }

        private int ParseInt(
            string? value)
        {
            if (int.TryParse(value, out int r))
                return r;

            return 0;
        }
    }
}