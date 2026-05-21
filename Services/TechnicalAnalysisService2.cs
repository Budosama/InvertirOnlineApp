// =============================================
// Services/TechnicalAnalysisService.cs
// =============================================

using System.Globalization;
using System.Text.Json;
using InvertirOnlineApp.Models;

namespace InvertirOnlineApp.Services
{
    public class TechnicalAnalysisService2
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public TechnicalAnalysisService2(
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

        public async Task<EmpresaTecnica?> GetEmpresaAsync(
            string symbol)
        {
            try
            {
                // =============================================
                // RSI
                // =============================================

                var rsi =
                    await GetIndicatorValue(
                        symbol,
                        "RSI",
                        "RSI");

                // =============================================
                // SMA 50
                // =============================================

                var sma50 =
                    await GetIndicatorValue(
                        symbol,
                        "SMA",
                        "SMA",
                        additional:
                        "&time_period=50&series_type=close");

                // =============================================
                // SMA 200
                // =============================================

                var sma200 =
                    await GetIndicatorValue(
                        symbol,
                        "SMA",
                        "SMA",
                        additional:
                        "&time_period=200&series_type=close");

                // =============================================
                // EMA 20
                // =============================================

                var ema20 =
                    await GetIndicatorValue(
                        symbol,
                        "EMA",
                        "EMA",
                        additional:
                        "&time_period=20&series_type=close");

                // =============================================
                // MACD
                // =============================================

                var macdData =
                    await GetMacd(symbol);

                // =============================================
                // PRICE
                // =============================================

                var price =
                    await GetPrice(symbol);

                if (price == null)
                    return null;

                // =============================================
                // BUILD MODEL
                // =============================================

                var empresa =
                    new EmpresaTecnica
                    {
                        Symbol = symbol,

                        Price = price,

                        RSI = rsi,

                        SMA50 = sma50,

                        SMA200 = sma200,

                        EMA20 = ema20,

                        MACD = macdData.macd,

                        MACDSignal = macdData.signal
                    };

                // =============================================
                // SCORE
                // =============================================

                empresa.TechnicalScore =
                    CalcularScore(empresa);

                empresa.Recommendation =
                    GetRecommendation(
                        empresa.TechnicalScore);

                return empresa;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return null;
            }
        }

        // =============================================
        // INDICATORS
        // =============================================

        private async Task<decimal?> GetIndicatorValue(
            string symbol,
            string function,
            string propertyName,
            string additional = "")
        {
            try
            {
                var url =
                    $"https://www.alphavantage.co/query?" +
                    $"function={function}" +
                    $"&symbol={symbol}" +
                    $"&interval=daily" +
                    $"{additional}" +
                    $"&apikey={ApiKey}";

                var response =
                    await _httpClient.GetStringAsync(url);

                using var doc =
                    JsonDocument.Parse(response);

                var root =
                    doc.RootElement;

                var technicalAnalysis =
                    root.EnumerateObject()
                        .FirstOrDefault(x =>
                            x.Name.Contains(
                                "Technical Analysis"));

                if (technicalAnalysis.Value
                    .ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                var latest =
                    technicalAnalysis.Value
                        .EnumerateObject()
                        .FirstOrDefault();

                if (latest.Value
                    .ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                if (!latest.Value.TryGetProperty(
                    propertyName,
                    out var value))
                {
                    return null;
                }

                return ParseDecimal(
                    value.GetString());
            }
            catch
            {
                return null;
            }
        }

        // =============================================
        // MACD
        // =============================================

        private async Task<(decimal? macd,
                            decimal? signal)>
            GetMacd(string symbol)
        {
            try
            {
                var url =
                    $"https://www.alphavantage.co/query?" +
                    $"function=MACD" +
                    $"&symbol={symbol}" +
                    $"&interval=daily" +
                    $"&series_type=close" +
                    $"&apikey={ApiKey}";

                var response =
                    await _httpClient.GetStringAsync(url);

                using var doc =
                    JsonDocument.Parse(response);

                var root =
                    doc.RootElement;

                var technicalAnalysis =
                    root.EnumerateObject()
                        .FirstOrDefault(x =>
                            x.Name.Contains(
                                "Technical Analysis"));

                var latest =
                    technicalAnalysis.Value
                        .EnumerateObject()
                        .FirstOrDefault();

                var macd =
                    ParseDecimal(
                        latest.Value
                            .GetProperty("MACD")
                            .GetString());

                var signal =
                    ParseDecimal(
                        latest.Value
                            .GetProperty("MACD_Signal")
                            .GetString());

                return (macd, signal);
            }
            catch
            {
                return (null, null);
            }
        }

        // =============================================
        // CURRENT PRICE
        // =============================================

        private async Task<decimal?> GetPrice(
            string symbol)
        {
            try
            {
                var url =
                    $"https://www.alphavantage.co/query?" +
                    $"function=GLOBAL_QUOTE" +
                    $"&symbol={symbol}" +
                    $"&apikey={ApiKey}";

                var response =
                    await _httpClient.GetStringAsync(url);

                using var doc =
                    JsonDocument.Parse(response);

                var price =
                    doc.RootElement
                        .GetProperty("Global Quote")
                        .GetProperty("05. price")
                        .GetString();

                return ParseDecimal(price);
            }
            catch
            {
                return null;
            }
        }

        // =============================================
        // SCORE
        // =============================================

        private int CalcularScore(
            EmpresaTecnica e)
        {
            double score = 0;

            // =============================================
            // RSI
            // =============================================

            if (e.RSI >= 45 &&
                e.RSI <= 65)
            {
                score += 20;
            }
            else if (e.RSI >= 35 &&
                     e.RSI <= 75)
            {
                score += 10;
            }

            // =============================================
            // PRICE VS SMA50
            // =============================================

            if (e.Price > e.SMA50)
                score += 15;

            // =============================================
            // SMA50 VS SMA200
            // =============================================

            if (e.SMA50 > e.SMA200)
                score += 25;

            // =============================================
            // PRICE VS EMA20
            // =============================================

            if (e.Price > e.EMA20)
                score += 15;

            // =============================================
            // MACD
            // =============================================

            if (e.MACD > e.MACDSignal)
                score += 20;

            // =============================================
            // EXTRA MOMENTUM
            // =============================================

            if (e.Price > e.SMA50 &&
                e.SMA50 > e.SMA200 &&
                e.MACD > e.MACDSignal)
            {
                score += 5;
            }

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
            if (score >= 80)
                return "🟢 STRONG BUY";

            if (score >= 60)
                return "🟡 BUY";

            if (score >= 40)
                return "🟠 HOLD";

            return "🔴 SELL";
        }

        // =============================================
        // PARSE
        // =============================================

        private decimal? ParseDecimal(
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
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
    }
}