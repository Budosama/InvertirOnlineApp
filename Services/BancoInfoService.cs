using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using InvertirOnlineApp.Models;

public class BancoInfoService
{
    private readonly HttpClient _httpClient;

    public BancoInfoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<BancoInfo>> GetBancoInfoAsync()
    {
        var response = await _httpClient.GetAsync("https://api.argentinadatos.com/v1/finanzas/tasas/plazoFijo");
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();

        var bancos = new List<BancoInfo>();
        bancos = JsonSerializer.Deserialize<List<BancoInfo>>(jsonResponse);

        if (bancos != null)
        {
            bancos = bancos.OrderByDescending(b => b.tnaClientes ?? 0).ToList();
        }
        else
        {
            Console.WriteLine("La deserialización devolvió un objeto nulo.");
        }

        return bancos!;
    }

}