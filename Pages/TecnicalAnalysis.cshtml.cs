// =============================================
// Pages/TechnicalAnalysis.cshtml.cs
// =============================================

using InvertirOnlineApp.Models;
using InvertirOnlineApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestSharp;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InvertirOnlineApp.Pages
{
    public class TechnicalAnalysisModel : PageModel
    {
        private readonly TechnicalAnalysisService _service;

        public TechnicalAnalysisModel(
            TechnicalAnalysisService service)
        {
            _service = service;
        }

        // =============================================
        // DATA
        // =============================================

        public List<EmpresaTecnica> Empresas { get; set; }
            = new();

        public string? SelectedSymbol { get; set; }

        // =============================================
        // AVAILABLE COMPANIES
        // =============================================

        public List<(string Symbol, string Name)>
            AvailableCompanies
        { get; set; }
                = new()
                {
                    ("AAPL", "Apple"),
                    ("MSFT", "Microsoft"),
                    ("GOOGL", "Alphabet"),
                    ("AMZN", "Amazon"),
                    ("META", "Meta"),
                    ("NVDA", "NVIDIA"),
                    ("AMD", "AMD"),
                    ("TSM", "TSMC"),
                    ("ASML", "ASML"),

                    ("COST", "Costco"),
                    ("WMT", "Walmart"),
                    ("KO", "Coca-Cola"),
                    ("PEP", "PepsiCo"),
                    ("MCD", "McDonald's"),

                    ("JPM", "JPMorgan"),
                    ("V", "Visa"),
                    ("MA", "Mastercard"),
                    ("BRK-B", "Berkshire Hathaway"),

                    ("JNJ", "Johnson & Johnson"),
                    ("LLY", "Eli Lilly"),

                    ("CAT", "Caterpillar"),
                    ("XOM", "Exxon Mobil"),

                    ("SPY", "SPDR S&P 500 ETF"),
                    ("QQQ", "Nasdaq QQQ ETF")
                };

        // =============================================
        // GET
        // =============================================

        public async Task OnGetAsync(
            string? symbol = null)
        {
            ViewData["Title"] =
                "Technical Analysis";

            SelectedSymbol = symbol;

            // =============================================
            // SINGLE COMPANY
            // =============================================

            if (!string.IsNullOrWhiteSpace(symbol))
            {
                var empresa =
                    await _service.GetEmpresaAsync(
                        symbol);

                if (empresa != null)
                {
                    Empresas.Add(empresa);
                }

                return;
            }

            string? accessToken = null;
            var tokenJson = HttpContext.Session.GetString("AuthToken");
            if (tokenJson != null)
            {
                var tokenObject = JsonSerializer.Deserialize<TokenResponse>(tokenJson);
                accessToken = tokenObject?.AccessToken;
            }

            var defaultSymbols =
                new List<string>();

            if (!string.IsNullOrEmpty(accessToken))
            {
                var client = new RestClient("https://api.invertironline.com");
                var request = new RestRequest("/api/v2/portafolio/argentina", Method.Get);
                request.AddHeader("Authorization", $"Bearer {accessToken}");

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        try
                        {
                            var portafolioResponse = JsonSerializer.Deserialize<PortafolioResponse>(response.Content);

                            if (portafolioResponse != null)
                            {
                                if (portafolioResponse.activos != null && portafolioResponse.activos.Any())
                                {
                                    foreach (var item in portafolioResponse.activos)
                                    {
                                        defaultSymbols.Add(item.titulo.simbolo);
                                    }
                                }
                            }
                        }
                        catch (JsonException ex)
                        {

                        }
                    }
                }
            }
            else 
            {
                defaultSymbols =
                new List<string>
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

            foreach (var s in defaultSymbols)
            {
                var empresa =
                    await _service.GetEmpresaAsync(s);

                if (empresa != null)
                {
                    Empresas.Add(empresa);
                }

                // evita rate limit
                await Task.Delay(10000);
            }

            Empresas =
                Empresas
                    .OrderByDescending(x =>
                        x.TechnicalScore)
                    .ToList();
        }

        // =============================================
        // COLORS
        // =============================================

        public string GetColorClassGood(
            decimal? value,
            decimal good,
            decimal warning)
        {
            if (!value.HasValue)
                return "metric-warning";

            if (value >= good)
                return "metric-good";

            if (value >= warning)
                return "metric-warning";

            return "metric-bad";
        }

        public string GetColorClassLow(
            decimal? value,
            decimal good,
            decimal warning)
        {
            if (!value.HasValue)
                return "metric-warning";

            if (value <= good)
                return "metric-good";

            if (value <= warning)
                return "metric-warning";

            return "metric-bad";
        }

        // =============================================
        // RSI
        // =============================================

        public string GetRsiClass(
            decimal? rsi)
        {
            if (!rsi.HasValue)
                return "metric-warning";

            if (rsi >= 45 && rsi <= 65)
                return "metric-good";

            if ((rsi >= 35 && rsi < 45) ||
                (rsi > 65 && rsi <= 75))
            {
                return "metric-warning";
            }

            return "metric-bad";
        }

        // =============================================
        // TREND
        // =============================================

        public string GetTrendClass(
            decimal? current,
            decimal? average)
        {
            if (!current.HasValue ||
                !average.HasValue)
            {
                return "metric-warning";
            }

            if (current > average)
                return "metric-good";

            return "metric-bad";
        }

        // =============================================
        // MACD
        // =============================================

        public string GetMacdClass(
            decimal? macd,
            decimal? signal)
        {
            if (!macd.HasValue ||
                !signal.HasValue)
            {
                return "metric-warning";
            }

            // bullish crossover
            if (macd > signal)
                return "metric-good";

            // bearish crossover
            if (macd < signal)
                return "metric-bad";

            return "metric-warning";
        }

        // =============================================
        // MOVING AVERAGES
        // =============================================

        public string GetMovingAverageClass(
            decimal? price,
            decimal? average)
        {
            if (!price.HasValue ||
                !average.HasValue)
            {
                return "metric-warning";
            }

            // price above MA
            if (price > average)
                return "metric-good";

            // near average
            var diff =
                Math.Abs(price.Value - average.Value)
                / average.Value;

            if (diff <= 0.02m)
                return "metric-warning";

            // below average
            return "metric-bad";
        }

        // =============================================
        // TREND
        // =============================================

        public string GetTrendClass(
            string? trend)
        {
            if (string.IsNullOrWhiteSpace(
                trend))
            {
                return "metric-warning";
            }

            trend =
                trend.ToLower();

            if (trend.Contains("bull"))
                return "metric-good";

            if (trend.Contains("bear"))
                return "metric-bad";

            return "metric-warning";
        }

        // =============================================
        // VOLATILITY
        // =============================================

        public string GetVolatilityClass(
            decimal? volatility)
        {
            if (!volatility.HasValue)
                return "metric-warning";

            // low volatility
            if (volatility <= 2)
                return "metric-good";

            // medium volatility
            if (volatility <= 4)
                return "metric-warning";

            // high volatility
            return "metric-bad";
        }

        // =============================================
        // SCORE
        // =============================================

        public string GetScoreClass(
            int score)
        {
            if (score >= 80)
                return "score-excellent";

            if (score >= 60)
                return "score-good";

            if (score >= 40)
                return "score-warning";

            return "score-bad";
        }

        // =============================================
        // RECOMMENDATION
        // =============================================

        public string GetRecommendationClass(
            string? recommendation)
        {
            if (string.IsNullOrWhiteSpace(
                recommendation))
            {
                return "text-secondary";
            }

            recommendation =
                recommendation.ToLower();

            if (recommendation.Contains(
                "strong buy"))
            {
                return "text-success fw-bold";
            }

            if (recommendation.Contains(
                "buy"))
            {
                return "text-success";
            }

            if (recommendation.Contains(
                "hold"))
            {
                return "text-warning fw-bold";
            }

            return "text-danger fw-bold";
        }

        // =============================================
        // FORMATTERS
        // =============================================

        public string FormatDecimal(
            decimal? value)
        {
            if (!value.HasValue)
                return "-";

            return value.Value.ToString("0.00");
        }

        public string FormatPrice(
            decimal? value)
        {
            if (!value.HasValue)
                return "-";

            return $"${value.Value:0.00}";
        }

        public class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }
        }
    }
}