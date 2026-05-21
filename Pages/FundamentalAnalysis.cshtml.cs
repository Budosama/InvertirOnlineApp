// =============================================
// Pages/FundamentalAnalysis.cshtml.cs
// =============================================

using InvertirOnlineApp.Models;
using InvertirOnlineApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

        public async Task OnGetAsync()
        {
            ViewData["Title"] =
                "Fundamental Analysis";

            Empresas =
                await _service.GetEmpresasAsync();
        }

        // =============================================
        // MÉTRICAS DONDE MÁS ALTO = MEJOR
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

        // =============================================
        // MÉTRICAS DONDE MÁS BAJO = MEJOR
        // =============================================

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
        // SCORE GENERAL
        // =============================================

        public string GetScoreClass(int score)
        {
            if (score >= 8)
                return "score-excellent";

            if (score >= 5)
                return "score-good";

            return "score-bad";
        }

        // =============================================
        // TEXTO RECOMENDACIÓN
        // =============================================

        public string GetRecommendationClass(string? recommendation)
        {
            if (string.IsNullOrWhiteSpace(recommendation))
                return "text-secondary";

            recommendation =
                recommendation.ToLower();

            if (recommendation.Contains("strong buy"))
                return "text-success fw-bold";

            if (recommendation.Contains("buy"))
                return "text-success";

            if (recommendation.Contains("hold"))
                return "text-warning fw-bold";

            if (recommendation.Contains("sell"))
                return "text-danger fw-bold";

            return "text-light";
        }

        // =============================================
        // FORMATEO MARKET CAP
        // =============================================

        public string FormatMarketCap(decimal? value)
        {
            if (!value.HasValue)
                return "-";

            decimal number = value.Value;

            if (number >= 1_000_000_000_000)
            {
                return
                    $"{number / 1_000_000_000_000:0.##}T";
            }

            if (number >= 1_000_000_000)
            {
                return
                    $"{number / 1_000_000_000:0.##}B";
            }

            if (number >= 1_000_000)
            {
                return
                    $"{number / 1_000_000:0.##}M";
            }

            return number.ToString("0.##");
        }

        // =============================================
        // FORMATEO %
        // =============================================

        public string FormatPercent(decimal? value)
        {
            if (!value.HasValue)
                return "-";

            return $"{value:0.0}%";
        }

        // =============================================
        // FORMATEO DECIMAL
        // =============================================

        public string FormatDecimal(decimal? value)
        {
            if (!value.HasValue)
                return "-";

            return value.Value.ToString("0.00");
        }
    }
}