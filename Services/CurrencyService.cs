using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

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