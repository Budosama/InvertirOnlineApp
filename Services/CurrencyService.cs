using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Globalization;

public class CurrencyService
{
    private readonly HttpClient _httpClient;

    public CurrencyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<decimal?> GetBTCValueInUSDAsync()
    {
        var response = await _httpClient.GetAsync("https://api.coindesk.com/v1/bpi/currentprice.json");
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        var bitcoinPriceResponse = JsonSerializer.Deserialize<BitcoinPriceResponse>(jsonResponse);

        if (bitcoinPriceResponse != null)
        {
            if (bitcoinPriceResponse.bpi != null && bitcoinPriceResponse.bpi.USD != null)
            {
                decimal? btcToUsd = bitcoinPriceResponse.bpi.USD.rate_float;
                return btcToUsd;
            }
            else
            {
                Console.WriteLine("Bpi o USD es null.");
            }
        }
        else
        {
            Console.WriteLine("La deserialización devolvió un objeto nulo.");
        }

        return 0;
    }

    public async Task<decimal?> GetBTCValueYesterdayInUSDAsync()
    {
        var yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        var today = DateTime.Now.ToString("yyyy-MM-dd");

        var url = $"https://api.coindesk.com/v1/bpi/historical/close.json?start={yesterday}&end={yesterday}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();

        var historicalPriceResponse = JsonSerializer.Deserialize<HistoricalBitcoinPriceResponse>(jsonResponse);

        if (historicalPriceResponse != null && historicalPriceResponse.bpi != null)
        {
            if (historicalPriceResponse.bpi.ContainsKey(yesterday))
            {
                return historicalPriceResponse.bpi[yesterday];
            }
            else
            {
                Console.WriteLine($"No se encontró información para la fecha {yesterday}.");
            }
        }
        else
        {
            Console.WriteLine("La deserialización devolvió un objeto nulo.");
        }

        return 0;
    }

    public async Task<decimal> GetUSDTValueInARSAsync()
    {
        var response = await _httpClient.GetAsync("https://criptoya.com/api/binance/USDT/ARS/0.1");
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();

        var usdtToArsResponse = JsonSerializer.Deserialize<UsdtToArsResponse>(jsonResponse);

        if (usdtToArsResponse != null)
        {
            decimal usdtToArs = usdtToArsResponse.totalAsk; 
            return usdtToArs;
        }
        else
        {
            Console.WriteLine("La deserialización devolvió un objeto nulo.");
        }

        return 0;
    }

    public async Task<decimal> GetUSDTValueInARSYesterdayAsync()
    {
        var yesterday = DateTime.Now.AddDays(-1);
        var startOfDay = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0, DateTimeKind.Utc);
        var endOfDay = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59, DateTimeKind.Utc);
        
        long startTimestamp = new DateTimeOffset(startOfDay).ToUnixTimeMilliseconds();
        long endTimestamp = new DateTimeOffset(endOfDay).ToUnixTimeMilliseconds();

        string url = $"https://api.binance.com/api/v3/klines?symbol=USDTARS&interval=1d&startTime={startTimestamp}&endTime={endTimestamp}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<List<List<object>>>(jsonResponse);

        if (data != null && data.Count > 0)
        {
            // La posición 4 contiene el precio de cierre, que es un string dentro del array
            var closePriceString = data[0][4].ToString();
            
            // Convertimos el precio de cierre a decimal
            if (decimal.TryParse(closePriceString!.Replace(".",","), out decimal closePrice))
            {
                return closePrice;
            }
            else
            {
                Console.WriteLine("No se pudo convertir el precio de cierre a decimal.");
            }
        }
        else
        {
            Console.WriteLine("No se encontró data válida para el valor de USDT en ARS de ayer.");
        }

        return 0;
    }

}

public class UsdtToArsResponse
{
    public decimal ask { get; set; }
    public decimal totalAsk { get; set; }
    public decimal bid { get; set; }
    public decimal totalBid { get; set; }
    public long time { get; set; }
}

public class BitcoinPriceResponse
{
    public TimeInfo time { get; set; } = new TimeInfo();
    public string disclaimer { get; set; } = string.Empty;
    public Bpi bpi { get; set; } = new Bpi();
}

public class TimeInfo
{
    public string updated { get; set; } = string.Empty;
    public string updatedISO { get; set; } = string.Empty;
    public string updateduk { get; set; } = string.Empty;
}

public class Bpi
{
    public CurrencyInfo USD { get; set; } = new CurrencyInfo();
    public CurrencyInfo GBP { get; set; } = new CurrencyInfo();
    public CurrencyInfo EUR { get; set; } = new CurrencyInfo();
}

public class CurrencyInfo
{
    public string code { get; set; } = string.Empty;
    public string symbol { get; set; } = string.Empty;
    public string rate { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public decimal? rate_float { get; set; }
}