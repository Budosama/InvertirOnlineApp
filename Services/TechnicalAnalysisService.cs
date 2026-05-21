// =============================================
// Services/TechnicalAnalysisService.cs
// =============================================

using System.Globalization;
using System.Text.Json;
using InvertirOnlineApp.Models;

namespace InvertirOnlineApp.Services
{
    public class TechnicalAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public TechnicalAnalysisService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        private string ApiKey =>
            _configuration["TwelveData:ApiKey"]!;

        // =============================================
        // MAIN
        // =============================================

        public async Task<EmpresaTecnica?> GetEmpresaAsync(
            string symbol)
        {
            try
            {
                // =============================================
                // GET HISTORICAL DATA
                // =============================================

                var data =
                    await GetHistoricalData(symbol);

                if (data == null ||
                    data.Values.Count < 200)
                {
                    return null;
                }

                // =============================================
                // ORDER ASCENDING
                // =============================================

                var ordered =
                    data.Values
                        .OrderBy(x => x.DateTime)
                        .ToList();

                var closes =
                    ordered
                        .Select(x => x.Close)
                        .ToList();

                var latest =
                    ordered.Last();

                // =============================================
                // CALCULATE INDICATORS
                // =============================================

                var sma20 =
                    CalculateSma(closes, 20);

                var sma50 =
                    CalculateSma(closes, 50);

                var sma200 =
                    CalculateSma(closes, 200);

                var ema20 =
                    CalculateEma(closes, 20);

                var rsi =
                    CalculateRsi(closes);

                var macdData =
                    CalculateMacd(closes);

                var atr =
                    CalculateAtr(ordered);

                // =============================================
                // BUILD MODEL
                // =============================================

                var empresa =
                new EmpresaTecnica
                {
                    Symbol = symbol,

                    CompanyName = GetCompanyName(symbol),

                    Sector = GetSector(symbol),

                    Price = latest.Close,

                    RSI = rsi,

                    SMA50 = sma50,

                    SMA200 = sma200,

                    EMA20 = ema20,

                    MACD = macdData.macd,

                    MACDSignal = macdData.signal
                };

                empresa.Trend =
                    GetTrend(empresa);

                empresa.Volatility =
                    CalculateVolatility(data.Values);

                empresa.Signal =
                    GetSignal(empresa);

                // =============================================
                // SCORE
                // =============================================

                empresa.TechnicalScore =
                    CalcularScore(empresa);

                empresa.Recommendation =
                    GetRecommendation(
                        empresa.TechnicalScore);

                empresa.Signal =
                    GetSignal(empresa);

                return empresa;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return null;
            }
        }

        // =============================================
        // GET HISTORICAL DATA
        // =============================================

        private async Task<TwelveDataResponse?>
            GetHistoricalData(string symbol)
        {
            try
            {
                var url =
                    $"https://api.twelvedata.com/time_series" +
                    $"?symbol={symbol}" +
                    $"&interval=1day" +
                    $"&outputsize=300" +
                    $"&apikey={ApiKey}";

                var response =
                    await _httpClient.GetStringAsync(url);

                return JsonSerializer.Deserialize
                    <TwelveDataResponse>(
                        response,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
            }
            catch
            {
                return null;
            }
        }

        private string GetTrend(EmpresaTecnica e)
        {
            if (e.Price > e.SMA50 &&
                e.SMA50 > e.SMA200)
            {
                return "Bullish";
            }

            if (e.Price < e.SMA50 &&
                e.SMA50 < e.SMA200)
            {
                return "Bearish";
            }

            return "Neutral";
        }

        private decimal? CalculateVolatility(
            List<PriceValue> values)
        {
            try
            {
                var closes =
                    values
                        .Take(30)
                        .Select(x => x.Close)
                        .ToList();

                if (closes.Count < 2)
                    return null;

                var returns =
                    new List<decimal>();

                for (int i = 1; i < closes.Count; i++)
                {
                    var change =
                        ((closes[i - 1] - closes[i])
                         / closes[i]) * 100;

                    returns.Add(Math.Abs(change));
                }

                return Math.Round(
                    returns.Average(),
                    2);
            }
            catch
            {
                return null;
            }
        }

        private string GetSignal(
            EmpresaTecnica e)
        {
            if (e.TechnicalScore >= 80)
                return "Strong Buy";

            if (e.TechnicalScore >= 60)
                return "Buy";

            if (e.TechnicalScore >= 40)
                return "Hold";

            return "Sell";
        }

        private string GetCompanyName(
            string symbol)
        {
            return symbol switch
            {
                "AAPL" => "Apple",
                "MSFT" => "Microsoft",
                "NVDA" => "NVIDIA",
                "AMZN" => "Amazon",
                "META" => "Meta",
                "GOOGL" => "Alphabet",
                "TSM" => "TSMC",
                "ASML" => "ASML",
                "AMD" => "AMD",
                "SPY" => "SPDR S&P 500 ETF",
                "QQQ" => "Invesco QQQ",
                _ => symbol
            };
        }

        private string GetSector(
            string symbol)
        {
            return symbol switch
            {
                "AAPL" => "Technology",
                "MSFT" => "Technology",
                "NVDA" => "Semiconductors",
                "AMD" => "Semiconductors",
                "TSM" => "Semiconductors",
                "ASML" => "Semiconductors",
                "AMZN" => "Consumer",
                "META" => "Communication",
                "GOOGL" => "Communication",
                "SPY" => "ETF",
                "QQQ" => "ETF",
                _ => "Unknown"
            };
        }

        // =============================================
        // SMA
        // =============================================

        private decimal? CalculateSma(
            List<decimal> values,
            int period)
        {
            if (values.Count < period)
                return null;

            return values
                .TakeLast(period)
                .Average();
        }

        // =============================================
        // EMA
        // =============================================

        private decimal? CalculateEma(
            List<decimal> values,
            int period)
        {
            if (values.Count < period)
                return null;

            var multiplier =
                2m / (period + 1);

            decimal ema =
                values.Take(period).Average();

            foreach (var price in values.Skip(period))
            {
                ema =
                    ((price - ema) * multiplier)
                    + ema;
            }

            return ema;
        }

        // =============================================
        // RSI
        // =============================================

        private decimal? CalculateRsi(
            List<decimal> closes,
            int period = 14)
        {
            if (closes.Count <= period)
                return null;

            decimal gains = 0;
            decimal losses = 0;

            for (int i = closes.Count - period;
                 i < closes.Count;
                 i++)
            {
                var change =
                    closes[i] - closes[i - 1];

                if (change > 0)
                    gains += change;
                else
                    losses += Math.Abs(change);
            }

            if (losses == 0)
                return 100;

            var avgGain =
                gains / period;

            var avgLoss =
                losses / period;

            var rs =
                avgGain / avgLoss;

            return
                100 - (100 / (1 + rs));
        }

        // =============================================
        // MACD
        // =============================================

        private (
            decimal? macd,
            decimal? signal,
            decimal? histogram)
            CalculateMacd(
                List<decimal> closes)
        {
            var ema12 =
                CalculateEma(closes, 12);

            var ema26 =
                CalculateEma(closes, 26);

            if (!ema12.HasValue ||
                !ema26.HasValue)
            {
                return (null, null, null);
            }

            var macd =
                ema12.Value - ema26.Value;

            // Simplificado
            var signal =
                macd * 0.8m;

            var histogram =
                macd - signal;

            return (
                macd,
                signal,
                histogram);
        }

        // =============================================
        // ATR
        // =============================================

        private decimal? CalculateAtr(
            List<PriceValue> data,
            int period = 14)
        {
            if (data.Count <= period)
                return null;

            var trueRanges =
                new List<decimal>();

            for (int i = 1; i < data.Count; i++)
            {
                var current =
                    data[i];

                var previous =
                    data[i - 1];

                var tr1 =
                    current.High - current.Low;

                var tr2 =
                    Math.Abs(
                        current.High -
                        previous.Close);

                var tr3 =
                    Math.Abs(
                        current.Low -
                        previous.Close);

                var tr =
                    Math.Max(
                        tr1,
                        Math.Max(tr2, tr3));

                trueRanges.Add(tr);
            }

            return trueRanges
                .TakeLast(period)
                .Average();
        }

        // =============================================
        // SCORE
        // =============================================

        private int CalcularScore(EmpresaTecnica e)
        {
            double score = 50;

            // RSI
            if (e.RSI >= 45 && e.RSI <= 60)
                score += 10;
            else if (e.RSI > 75)
                score -= 15;
            else if (e.RSI < 30)
                score += 5;

            // Trend
            if (e.Price > e.SMA50)
                score += 10;
            else
                score -= 10;

            if (e.SMA50 > e.SMA200)
                score += 15;
            else
                score -= 15;

            // EMA
            if (e.Price > e.EMA20)
                score += 10;

            // MACD
            if (e.MACD > e.MACDSignal)
                score += 10;
            else
                score -= 10;

            // Volatility
            if (e.Volatility > 4)
                score -= 15;
            else if (e.Volatility > 3)
                score -= 8;

            return (int)Math.Max(0, Math.Min(100, score));
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