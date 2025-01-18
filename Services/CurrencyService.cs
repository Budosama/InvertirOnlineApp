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
        var yesterday = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

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
        string apiUrl = "https://api.binance.com/api/v3/klines";
        string symbol = "USDTARS";
        string interval = "1d";
        long yesterdayTimestamp = ((DateTimeOffset)DateTime.UtcNow.AddDays(-1)).ToUnixTimeMilliseconds();

        var response = await _httpClient.GetAsync($"{apiUrl}?symbol={symbol}&interval={interval}&startTime={yesterdayTimestamp}&limit=1");
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();

        // Deserializamos la respuesta como una lista de listas
        var binanceResponse = JsonSerializer.Deserialize<List<List<JsonElement>>>(jsonResponse);

        if (binanceResponse != null && binanceResponse.Any())
        {
            // El precio de cierre se encuentra en la posición 4
            var closingPriceElement = binanceResponse[0][4];

            // Convertir el elemento a decimal de forma segura
            if (closingPriceElement.ValueKind == JsonValueKind.String)
            {
                return decimal.Parse(closingPriceElement.GetString()!, CultureInfo.InvariantCulture);
            }
            else if (closingPriceElement.ValueKind == JsonValueKind.Number)
            {
                return closingPriceElement.GetDecimal();
            }
        }

        Console.WriteLine("No se pudo obtener el precio de cierre.");
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