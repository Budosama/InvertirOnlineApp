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
            _configuration["FinancialModelingPrep:ApiKey"]!;

        public async Task<List<EmpresaFundamental>> GetEmpresasAsync()
        {
            List<string> symbols = new()
            {
                // BIG TECH
                "AAPL",
                "MSFT",
                "GOOGL",
                "AMZN",
                "META",
                "NVDA",
                "AMD",
                "ASML",
                "TSM",

                //// CONSUMER
                "COST",
                "WMT",
                "KO",
                "PEP",
                "MCD",

                //// FINANCIAL
                "BRK-B",
                "JPM",
                "V",
                "MA",

                //// HEALTHCARE
                "JNJ",
                "LLY",

                //// INDUSTRIAL
                "CAT",

                //// ENERGY
                "XOM",

                //// ETFs
                "SPY",
                "QQQ",

                "TSLA"
            };

            var empresas = new List<EmpresaFundamental>();

            foreach (var symbol in symbols)
            {
                try
                {
                    var empresa = await GetEmpresaAsync(symbol);

                    if (empresa != null)
                    {
                        empresa.FundamentalScore =
                            CalcularScore(empresa);

                        empresa.Recommendation =
                            GetRecommendation(
                                empresa.FundamentalScore);

                        empresas.Add(empresa);
                    }

                    await Task.Delay(5000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
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
            int score = 0;

            if (e.PERatio is > 0 and < 20)
                score += 10;

            if (e.ROE > 15)
                score += 10;

            if (e.ROA > 7)
                score += 10;

            if (e.NetMargin > 15)
                score += 10;

            if (e.OperatingMargin > 15)
                score += 10;

            if (e.RevenueGrowth > 10)
                score += 10;

            if (e.EPSGrowth > 10)
                score += 10;

            if (e.DebtToEquity < 1)
                score += 10;

            if (e.CurrentRatio > 1.5m)
                score += 10;

            if (e.PriceToBook < 5)
                score += 10;

            return score;
        }

        private string GetRecommendation(int score)
        {
            if (score >= 80)
                return "🟢 STRONG BUY";

            if (score >= 60)
                return "🟡 BUY";

            if (score >= 40)
                return "🟠 HOLD";

            return "🔴 RISKY";
        }
    }
}