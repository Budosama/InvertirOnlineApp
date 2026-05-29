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

            if (!string.IsNullOrWhiteSpace(symbol))
            {
                if (symbol != "Ninguna")
                {
                    Empresas = await _service.GetEmpresasAsync(symbol, accessToken);
                }

                return;
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