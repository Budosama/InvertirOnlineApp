using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using InvertirOnlineApp.Models;

public class BonosService
{
    private readonly HttpClient _httpClient;

    public BonosService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // public async Task<List<BonoInfo>> GetBonosAsync()
    // {
    //     var response = await _httpClient.GetAsync("https://api.iextrading.com/1.0/stock/market/bonds");
    //     response.EnsureSuccessStatusCode();
    //     var jsonResponse = await response.Content.ReadAsStringAsync();
    //     var bonos = new List<BonoInfo>();
    //     bonos = JsonSerializer.Deserialize<List<BonoInfo>>(jsonResponse);

    //     if (bonos != null)
    //     {
    //         bonos = bonos.OrderBy(b => b.Yield).ToList(); 
    //     }
    //     else
    //     {
    //         Console.WriteLine("La deserialización devolvió un objeto nulo.");
    //     }

    //     return bonos!;
    // }

}