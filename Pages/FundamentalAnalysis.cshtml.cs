using InvertirOnlineApp.Models;
using InvertirOnlineApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InvertirOnlineApp.Pages
{
    public class FundamentalAnalysisModel : PageModel
    {
        private readonly FundamentalAnalysisService _service;

        public FundamentalAnalysisModel(
            FundamentalAnalysisService service)
        {
            _service = service;
        }

        public List<EmpresaFundamental> Empresas { get; set; }
            = new();

        public string? SelectedSymbol { get; set; }

        public List<(string Symbol, string Name)> AvailableCompanies
            => new()
            {
                ("AAPL", "Apple"),
                ("MSFT", "Microsoft"),
                ("GOOGL", "Google"),
                ("AMZN", "Amazon"),
                ("META", "Meta"),
                ("NVDA", "NVIDIA"),
                ("AMD", "AMD"),
                ("ASML", "ASML"),
                ("TSM", "TSMC"),
                ("COST", "Costco"),
                ("WMT", "Walmart"),
                ("KO", "Coca-Cola"),
                ("PEP", "Pepsi"),
                ("MCD", "McDonald's"),
                ("BRK-B", "Berkshire Hathaway"),
                ("JPM", "JPMorgan"),
                ("V", "Visa"),
                ("MA", "Mastercard"),
                ("JNJ", "Johnson & Johnson"),
                ("LLY", "Eli Lilly"),
                ("CAT", "Caterpillar"),
                ("XOM", "Exxon Mobil"),
                ("SPY", "S&P 500 ETF"),
                ("QQQ", "Nasdaq ETF")
            };

        public async Task OnGetAsync(string? symbol = null)
        {
            ViewData["Title"] = "Fundamental Analysis";

            SelectedSymbol = symbol;

            string? accessToken = null;
            var tokenJson = HttpContext.Session.GetString("AuthToken");
            if (tokenJson != null)
            {
                var tokenObject = JsonSerializer.Deserialize<TokenResponse>(tokenJson);
                accessToken = tokenObject?.AccessToken;
            }           

            Empresas =
                await _service.GetEmpresasAsync(symbol, accessToken);
        }

        // =========================
        // COLORES
        // =========================

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

        public string GetScoreClass(int score)
        {
            if (score >= 80)
                return "score-excellent";

            if (score >= 60)
                return "score-good";

            return "score-bad";
        }

        public class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }
        }
    }
}