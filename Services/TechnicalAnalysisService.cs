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
                var data =
                    await GetHistoricalData(symbol);

                if (data == null ||
                    data.Values.Count < 220)
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
                // INDICATORS
                // =============================================

                var sma20 =
                    CalculateSma(closes, 20);

                var sma50 =
                    CalculateSma(closes, 50);

                var sma200 =
                    CalculateSma(closes, 200);

                var ema20 =
                    CalculateEma(closes, 20);

                var ema50 =
                    CalculateEma(closes, 50);

                var rsi =
                    CalculateRsi(closes);

                var macd =
                    CalculateMacd(closes);

                var volatility =
                    CalculateVolatility(ordered);

                var atr =
                    CalculateAtr(ordered);

                // =============================================
                // BUILD MODEL
                // =============================================

                var empresa =
                    new EmpresaTecnica
                    {
                        Symbol = symbol,

                        CompanyName =
                            GetCompanyName(symbol),

                        Sector =
                            GetSector(symbol),

                        Price =
                            latest.Close,

                        SMA20 = sma20,

                        SMA50 = sma50,

                        SMA200 = sma200,

                        EMA20 = ema20,

                        RSI = rsi,

                        MACD = macd.macd,

                        MACDSignal = macd.signal,

                        Volatility = volatility
                    };

                // =============================================
                // TREND
                // =============================================

                empresa.Trend =
                    GetTrend(empresa);

                // =============================================
                // SCORE
                // =============================================

                empresa.TechnicalScore =
                    CalculateScore(empresa);

                empresa.Signal =
                    GetSignal(empresa);

                empresa.Recommendation =
                    GetRecommendation(
                        empresa.TechnicalScore);

                empresa.Reasons =
                    GetReasons(empresa);

                return empresa;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return null;
            }
        }

        // =============================================
        // API
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

                return JsonSerializer.Deserialize<TwelveDataResponse>(
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

        // =============================================
        // TREND
        // =============================================

        private string GetTrend(
            EmpresaTecnica e)
        {
            if (e.Price > e.SMA50 &&
                e.SMA50 > e.SMA200)
            {
                return "Alcista";
            }

            if (e.Price < e.SMA50 &&
                e.SMA50 < e.SMA200)
            {
                return "Bajista";
            }

            return "Neutral";
        }

        // =============================================
        // VOLATILITY
        // =============================================

        private decimal? CalculateVolatility(
            List<PriceValue> values)
        {
            try
            {
                var closes =
                    values
                        .TakeLast(30)
                        .Select(x => x.Close)
                        .ToList();

                if (closes.Count < 2)
                    return null;

                var dailyReturns =
                    new List<decimal>();

                for (int i = 1; i < closes.Count; i++)
                {
                    var change =
                        ((closes[i] - closes[i - 1])
                        / closes[i - 1]) * 100m;

                    dailyReturns.Add(
                        Math.Abs(change));
                }

                return Math.Round(
                    dailyReturns.Average(),
                    2);
            }
            catch
            {
                return null;
            }
        }

        // =============================================
        // SIGNAL
        // =============================================

        private string GetSignal(
            EmpresaTecnica e)
        {
            if (e.TechnicalScore >= 85)
                return "Compra Fuerte";

            if (e.TechnicalScore >= 70)
                return "Compra";

            if (e.TechnicalScore >= 50)
                return "Mantener";

            if (e.TechnicalScore >= 35)
                return "Vender";

            return "Vender Fuerte";
        }

        // =============================================
        // REASONS
        // =============================================

        private List<string> GetReasons(
            EmpresaTecnica e)
        {
            var reasons =
                new List<string>();

            if (e.Price > e.SMA50)
                reasons.Add(
                    "Price above SMA50");

            if (e.SMA50 > e.SMA200)
                reasons.Add(
                    "Golden Cross structure");

            if (e.MACD > e.MACDSignal)
                reasons.Add(
                    "Bullish MACD");

            if (e.RSI >= 45 &&
                e.RSI <= 65)
            {
                reasons.Add(
                    "Healthy RSI");
            }

            if (e.RSI > 75)
            {
                reasons.Add(
                    "Overbought RSI");
            }

            if (e.Volatility > 4)
            {
                reasons.Add(
                    "High volatility");
            }

            if (e.Price < e.SMA200)
            {
                reasons.Add(
                    "Below SMA200");
            }

            return reasons;
        }

        // =============================================
        // COMPANY NAME
        // =============================================

        private string GetCompanyName(
            string symbol)
        {
            return symbol switch
            {
                // =============================================
                // BIG TECH
                // =============================================

                "AAPL" => "Apple",
                "MSFT" => "Microsoft",
                "GOOGL" => "Google",
                "AMZN" => "Amazon",
                "META" => "Meta",
                "NVDA" => "NVIDIA",
                "AMD" => "AMD",
                "TSM" => "TSMC",
                "ASML" => "ASML",
                "AVGO" => "Broadcom",
                "ORCL" => "Oracle",
                "CRM" => "Salesforce",
                "ADBE" => "Adobe",
                "INTC" => "Intel",
                "IBM" => "IBM",

                // =============================================
                // AI / SOFTWARE
                // =============================================

                "PLTR" => "Palantir",
                "SNOW" => "Snowflake",
                "CRWD" => "CrowdStrike",
                "PANW" => "Palo Alto Networks",
                "NET" => "Cloudflare",
                "DDOG" => "Datadog",
                "SHOP" => "Shopify",

                // =============================================
                // FINTECH / PAYMENTS
                // =============================================

                "MELI" => "MercadoLibre",
                "NU" => "Nu Holdings",
                "V" => "Visa",
                "MA" => "Mastercard",
                "PYPL" => "PayPal",
                "SQ" => "Block",
                "COIN" => "Coinbase",

                // =============================================
                // CONSUMER
                // =============================================

                "COST" => "Costco",
                "WMT" => "Walmart",
                "KO" => "Coca-Cola",
                "PEP" => "PepsiCo",
                "MCD" => "McDonald's",
                "SBUX" => "Starbucks",
                "NKE" => "Nike",
                "PG" => "Procter & Gamble",
                "HD" => "Home Depot",

                // =============================================
                // FINANCIALS
                // =============================================

                "BRK-B" => "Berkshire Hathaway",
                "JPM" => "JPMorgan Chase",
                "BAC" => "Bank of America",
                "GS" => "Goldman Sachs",
                "MS" => "Morgan Stanley",

                // =============================================
                // HEALTHCARE
                // =============================================

                "JNJ" => "Johnson & Johnson",
                "LLY" => "Eli Lilly",
                "UNH" => "UnitedHealth",
                "ABBV" => "AbbVie",
                "PFE" => "Pfizer",

                // =============================================
                // INDUSTRIALS
                // =============================================

                "CAT" => "Caterpillar",
                "GE" => "General Electric",
                "RTX" => "RTX",
                "BA" => "Boeing",

                // =============================================
                // ENERGY
                // =============================================

                "XOM" => "Exxon Mobil",
                "CVX" => "Chevron",
                "COP" => "ConocoPhillips",
                "SLB" => "Schlumberger",

                // =============================================
                // AUTOMOTIVE / EV
                // =============================================

                "TSLA" => "Tesla",
                "BYDDF" => "BYD",
                "RIVN" => "Rivian",

                // =============================================
                // ARGENTINA
                // =============================================

                "PAMP" => "Pampa Energia",
                "YPF" => "YPF",
                "GGAL" => "Grupo Galicia",
                "BMA" => "Banco Macro",
                "SUPV" => "Supervielle",
                "EDN" => "Edenor",
                "CEPU" => "Central Puerto",
                "TGS" => "Transportadora Gas del Sur",
                "MOLI" => "Molinos",

                // =============================================
                // ETFs
                // =============================================

                "SPY" => "SPDR S&P 500 ETF",
                "QQQ" => "Invesco QQQ ETF",
                "DIA" => "SPDR Dow Jones ETF",
                "IWM" => "iShares Russell 2000 ETF",
                "VTI" => "Vanguard Total Stock Market ETF",
                "ARKK" => "ARK Innovation ETF",
                "SOXX" => "iShares Semiconductor ETF",
                "XLE" => "Energy Select Sector ETF",

                // =============================================
                // CRYPTO
                // =============================================

                "BTC/USD" => "Bitcoin",
                "ETH/USD" => "Ethereum",
                "SOL/USD" => "Solana",
                "BNB/USD" => "BNB",
                "XRP/USD" => "XRP",
                "ADA/USD" => "Cardano",
                "DOGE/USD" => "Dogecoin",
                "AVAX/USD" => "Avalanche",
                "LINK/USD" => "Chainlink",
                "MATIC/USD" => "Polygon",
                "ARB/USD" => "Arbitrum",
                "OP/USD" => "Optimism",
                "NEAR/USD" => "Near Protocol",
                "ATOM/USD" => "Cosmos",
                "DOT/USD" => "Polkadot",

                _ => symbol
            };
        }

        // =============================================
        // SECTOR
        // =============================================

        private string GetSector(
            string symbol)
        {
            return symbol switch
            {
                // =============================================
                // BIG TECH
                // =============================================

                "AAPL" => "Technology",
                "MSFT" => "Technology",
                "GOOGL" => "Communication Services",
                "META" => "Communication Services",
                "AMZN" => "Consumer Cyclical",

                // =============================================
                // SEMICONDUCTORS
                // =============================================

                "NVDA" => "Semiconductors",
                "AMD" => "Semiconductors",
                "TSM" => "Semiconductors",
                "ASML" => "Semiconductors",

                // =============================================
                // SOFTWARE / FINTECH
                // =============================================

                "PLTR" => "Software",
                "NU" => "Fintech",
                "MELI" => "E-Commerce",

                // =============================================
                // CONSUMER DEFENSIVE
                // =============================================

                "COST" => "Consumer Defensive",
                "WMT" => "Consumer Defensive",
                "KO" => "Consumer Defensive",
                "PEP" => "Consumer Defensive",
                "MCD" => "Consumer Cyclical",

                // =============================================
                // FINANCIALS
                // =============================================

                "BRK-B" => "Financial Services",
                "JPM" => "Financial Services",
                "V" => "Financial Services",
                "MA" => "Financial Services",

                // =============================================
                // HEALTHCARE
                // =============================================

                "JNJ" => "Healthcare",
                "LLY" => "Healthcare",

                // =============================================
                // INDUSTRIAL / ENERGY
                // =============================================

                "CAT" => "Industrials",
                "XOM" => "Energy",

                // =============================================
                // ETFs
                // =============================================

                "SPY" => "ETF",
                "QQQ" => "ETF",

                // =============================================
                // CRYPTO
                // =============================================

                "BTC/USD" => "Cryptocurrency",
                "ETH/USD" => "Cryptocurrency",
                "SOL/USD" => "Cryptocurrency",
                "BNB/USD" => "Cryptocurrency",
                "XRP/USD" => "Cryptocurrency",
                "ADA/USD" => "Cryptocurrency",
                "DOGE/USD" => "Cryptocurrency",
                "AVAX/USD" => "Cryptocurrency",
                "LINK/USD" => "Cryptocurrency",
                "MATIC/USD" => "Cryptocurrency",

                // =============================================
                // EXTRA TECH
                // =============================================

                "AVGO" => "Semiconductors",
                "ORCL" => "Technology",
                "CRM" => "Software",
                "ADBE" => "Software",
                "INTC" => "Semiconductors",
                "IBM" => "Technology",

                // =============================================
                // AI / CYBERSECURITY
                // =============================================

                "SNOW" => "Software",
                "CRWD" => "Cybersecurity",
                "PANW" => "Cybersecurity",
                "NET" => "Cloud Computing",
                "DDOG" => "Software",
                "SHOP" => "E-Commerce",

                // =============================================
                // EXTRA FINTECH
                // =============================================

                "PYPL" => "Fintech",
                "SQ" => "Fintech",
                "COIN" => "Crypto Exchange",

                // =============================================
                // EXTRA CONSUMER
                // =============================================

                "SBUX" => "Consumer Defensive",
                "NKE" => "Consumer Cyclical",
                "PG" => "Consumer Defensive",
                "HD" => "Consumer Cyclical",

                // =============================================
                // EXTRA FINANCIALS
                // =============================================

                "BAC" => "Financial Services",
                "GS" => "Financial Services",
                "MS" => "Financial Services",

                // =============================================
                // EXTRA HEALTHCARE
                // =============================================

                "UNH" => "Healthcare",
                "ABBV" => "Healthcare",
                "PFE" => "Healthcare",

                // =============================================
                // EXTRA INDUSTRIALS
                // =============================================

                "GE" => "Industrials",
                "RTX" => "Industrials",
                "BA" => "Industrials",

                // =============================================
                // EXTRA ENERGY
                // =============================================

                "CVX" => "Energy",
                "COP" => "Energy",
                "SLB" => "Energy",

                // =============================================
                // EV
                // =============================================

                "TSLA" => "Automotive",
                "BYDDF" => "Automotive",
                "RIVN" => "Automotive",

                // =============================================
                // ARGENTINA
                // =============================================

                "PAMP" => "Energy",
                "YPF" => "Energy",
                "GGAL" => "Financial Services",
                "BMA" => "Financial Services",
                "SUPV" => "Financial Services",
                "EDN" => "Utilities",
                "CEPU" => "Utilities",
                "TGS" => "Energy",
                "MOLI" => "Consumer Defensive",

                // =============================================
                // ETFs
                // =============================================

                "DIA" => "ETF",
                "IWM" => "ETF",
                "VTI" => "ETF",
                "ARKK" => "ETF",
                "SOXX" => "ETF",
                "XLE" => "ETF",

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

            return Math.Round(
                values
                    .TakeLast(period)
                    .Average(),
                2);
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

            foreach (var value in values.Skip(period))
            {
                ema =
                    ((value - ema) * multiplier)
                    + ema;
            }

            return Math.Round(ema, 2);
        }

        // =============================================
        // EMA SERIES
        // =============================================

        private List<decimal> CalculateEmaSeries(
            List<decimal> values,
            int period)
        {
            var result =
                new List<decimal>();

            var multiplier =
                2m / (period + 1);

            decimal ema =
                values.Take(period).Average();

            result.Add(ema);

            foreach (var value in values.Skip(period))
            {
                ema =
                    ((value - ema) * multiplier)
                    + ema;

                result.Add(ema);
            }

            return result;
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

            var rsi =
                100 - (100 / (1 + rs));

            return Math.Round(rsi, 2);
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
            if (closes.Count < 35)
            {
                return (null, null, null);
            }

            var ema12 =
                CalculateEmaSeries(closes, 12);

            var ema26 =
                CalculateEmaSeries(closes, 26);

            var offset =
                ema12.Count - ema26.Count;

            var alignedEma12 =
                ema12.Skip(offset).ToList();

            var macdSeries =
                alignedEma12.Zip(
                    ema26,
                    (a, b) => a - b)
                .ToList();

            var signalSeries =
                CalculateEmaSeries(macdSeries, 9);

            var macd =
                macdSeries.Last();

            var signal =
                signalSeries.Last();

            var histogram =
                macd - signal;

            return (
                Math.Round(macd, 2),
                Math.Round(signal, 2),
                Math.Round(histogram, 2));
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

            return Math.Round(
                trueRanges
                    .TakeLast(period)
                    .Average(),
                2);
        }

        // =============================================
        // SCORE
        // =============================================

        private int CalculateScore(
            EmpresaTecnica e)
        {
            double score = 50;

            // =============================================
            // TREND
            // =============================================

            if (e.Price > e.SMA20)
                score += 5;
            else
                score -= 5;

            if (e.Price > e.SMA50)
                score += 10;
            else
                score -= 10;

            if (e.SMA50 > e.SMA200)
                score += 15;
            else
                score -= 15;

            // =============================================
            // EMA
            // =============================================

            if (e.Price > e.EMA20)
                score += 10;
            else
                score -= 5;

            // =============================================
            // RSI
            // =============================================

            if (e.RSI >= 45 &&
                e.RSI <= 65)
            {
                score += 10;
            }
            else if (e.RSI > 75)
            {
                score -= 15;
            }
            else if (e.RSI < 30)
            {
                score += 5;
            }

            // =============================================
            // MACD
            // =============================================

            if (e.MACD > e.MACDSignal)
                score += 15;
            else
                score -= 10;

            // =============================================
            // VOLATILITY
            // =============================================

            if (e.Volatility > 5)
                score -= 15;
            else if (e.Volatility > 3)
                score -= 8;

            // =============================================
            // NORMALIZE
            // =============================================

            score =
                Math.Max(0,
                Math.Min(100, score));

            return (int)Math.Round(score);
        }

        // =============================================
        // RECOMMENDATION
        // =============================================

        private string GetRecommendation(
            int score)
        {
            if (score >= 85)
                return "🟢 COMPRAR FUERTE";

            if (score >= 70)
                return "🟢 COMPRAR";

            if (score >= 50)
                return "🟡 MANTENER";

            if (score >= 35)
                return "🟠 VENDER";

            return "🔴 VENDER FUERTE";
        }
    }
}