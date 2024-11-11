using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using InvertirOnlineApp.Models;

public class EconomiaService
{
    private readonly HttpClient _httpClient;

    public EconomiaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<VariableEconomica>> GetVariablesEconomicasAsync()
    {
        var response = await _httpClient.GetAsync("https://api.bcra.gob.ar/estadisticas/v2.0/PrincipalesVariables");
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();

        var apiResponse = JsonSerializer.Deserialize<ApiResponse>(jsonResponse);

        if (apiResponse == null)
        {
            Console.WriteLine("La deserialización devolvió un objeto nulo.");
            return new List<VariableEconomica>();
        }

        return apiResponse.results;
    }

}