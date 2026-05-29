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

        public List<(string Symbol, string Name, string Sector)> AvailableCompanies
        => new()
        {
            // =============================================
            // BIG TECH
            // =============================================

            ("AAPL", "Apple", "Tecnología"),
            ("MSFT", "Microsoft", "Tecnología"),
            ("GOOGL", "Google", "Tecnología"),
            ("AMZN", "Amazon", "Tecnología"),
            ("META", "Meta", "Tecnología"),
            ("NVDA", "NVIDIA", "Tecnología"),
            ("AMD", "AMD", "Tecnología"),
            ("TSM", "TSMC", "Tecnología"),
            ("ASML", "ASML", "Tecnología"),
            ("AVGO", "Broadcom", "Tecnología"),
            ("ORCL", "Oracle", "Tecnología"),
            ("CRM", "Salesforce", "Tecnología"),
            ("ADBE", "Adobe", "Tecnología"),
            ("INTC", "Intel", "Tecnología"),
            ("IBM", "IBM", "Tecnología"),

            // =============================================
            // AI / SOFTWARE
            // =============================================

            ("PLTR", "Palantir", "Software"),
            ("SNOW", "Snowflake", "Software"),
            ("CRWD", "CrowdStrike", "Software"),
            ("PANW", "Palo Alto Networks", "Software"),
            ("NET", "Cloudflare", "Software"),
            ("DDOG", "Datadog", "Software"),
            ("SHOP", "Shopify", "Software"),

            // =============================================
            // FINTECH / PAYMENTS
            // =============================================

            ("MELI", "Mercado Libre", "Pagos"),
            ("NU", "Nu Holdings", "Pagos"),
            ("V", "Visa", "Pagos"),
            ("MA", "Mastercard", "Pagos"),
            ("PYPL", "PayPal", "Pagos"),
            ("SQ", "Block", "Pagos"),
            ("COIN", "Coinbase", "Pagos"),

            // =============================================
            // CONSUMER
            // =============================================

            ("COST", "Costco", "Consumo"),
            ("WMT", "Walmart", "Consumo"),
            ("KO", "Coca-Cola", "Consumo"),
            ("PEP", "PepsiCo", "Consumo"),
            ("MCD", "McDonald's", "Consumo"),
            ("SBUX", "Starbucks", "Consumo"),
            ("NKE", "Nike", "Consumo"),
            ("PG", "Procter & Gamble", "Consumo"),
            ("HD", "Home Depot", "Consumo"),

            // =============================================
            // FINANCIALS
            // =============================================

            ("BRK-B", "Berkshire Hathaway", "Finanzas"),
            ("JPM", "JPMorgan", "Finanzas"),
            ("BAC", "Bank of America", "Finanzas"),
            ("GS", "Goldman Sachs", "Finanzas"),
            ("MS", "Morgan Stanley", "Finanzas"),

            // =============================================
            // HEALTHCARE
            // =============================================

            ("JNJ", "Johnson & Johnson", "Salud"),
            ("LLY", "Eli Lilly", "Salud"),
            ("UNH", "UnitedHealth", "Salud"),
            ("ABBV", "AbbVie", "Salud"),
            ("PFE", "Pfizer", "Salud"),

            // =============================================
            // INDUSTRIALS
            // =============================================

            ("CAT", "Caterpillar", "Industrial"),
            ("GE", "General Electric", "Industrial"),
            ("RTX", "RTX", "Industrial"),
            ("BA", "Boeing", "Industrial"),

            // =============================================
            // ENERGY
            // =============================================

            ("XOM", "Exxon Mobil", "Energía"),
            ("CVX", "Chevron", "Energía"),
            ("COP", "ConocoPhillips", "Energía"),
            ("SLB", "Schlumberger", "Energía"),

            // =============================================
            // AUTOMOTIVE / EV
            // =============================================

            ("TSLA", "Tesla", "Automotor"),
            ("BYDDF", "BYD", "Automotor"),
            ("RIVN", "Rivian", "Automotor"),

            // =============================================
            // ARGENTINA
            // =============================================

            ("PAMP", "Pampa Energia", "Argentina"),
            ("YPF", "YPF", "Argentina"),
            ("GGAL", "Grupo Galicia", "Argentina"),
            ("BMA", "Banco Macro", "Argentina"),
            ("SUPV", "Supervielle", "Argentina"),
            ("EDN", "Edenor", "Argentina"),
            ("CEPU", "Central Puerto", "Argentina"),
            ("TGS", "Transportadora Gas del Sur", "Argentina"),
            ("MOLI", "Molinos", "Argentina"),

            // =============================================
            // ETFs
            // =============================================

            ("SPY", "S&P 500 ETF", "ETF"),
            ("QQQ", "Nasdaq ETF", "ETF"),
            ("DIA", "Dow Jones ETF", "ETF"),
            ("IWM", "Russell 2000 ETF", "ETF"),
            ("VTI", "Total Stock Market ETF", "ETF"),
            ("ARKK", "ARK Innovation ETF", "ETF"),
            ("SOXX", "Semiconductor ETF", "ETF"),
            ("XLE", "Energy ETF", "ETF"),

            // =============================================
            // CRYPTO
            // =============================================

            ("BTC/USD", "Bitcoin", "Crypto"),
            ("ETH/USD", "Ethereum", "Crypto"),
            ("SOL/USD", "Solana", "Crypto"),
            ("BNB/USD", "BNB", "Crypto"),
            ("XRP/USD", "XRP", "Crypto"),
            ("ADA/USD", "Cardano", "Crypto"),
            ("DOGE/USD", "Dogecoin", "Crypto"),
            ("AVAX/USD", "Avalanche", "Crypto"),
            ("LINK/USD", "Chainlink", "Crypto"),
            ("MATIC/USD", "Polygon", "Crypto")
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
                if (symbol != "Ninguna") {
                    var empresa =
                    await _service.GetEmpresaAsync(
                        symbol);

                    if (empresa != null)
                    {
                        Empresas.Add(empresa);
                    }
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
                new List<string>{};
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

            if (trend.Contains("cista"))
                return "metric-good";

            if (trend.Contains("jistas"))
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