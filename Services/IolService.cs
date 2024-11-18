using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using InvertirOnlineApp.Models;
using RestSharp;

public class IolService
{
    private readonly HttpClient _httpClient;

    public IolService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<TituloV2>> GetCotizacionesAsync(string instrumento, string pais, string accessToken)
    {
        var url = $"/api/v2/cotizaciones-orleans/{instrumento}/{pais}/Operables?cotizacionInstrumentoModel.instrumento={instrumento}&cotizacionInstrumentoModel.pais={pais}";
        var client = new RestClient("https://api.invertironline.com");
        var request = new RestRequest(url, Method.Get);
        request.AddHeader("Authorization", $"Bearer {accessToken}");

        var response = await client.ExecuteAsync(request);
        var cotizaciones = new List<TituloV2>();

        if (response.IsSuccessful)
        {
            if (!string.IsNullOrEmpty(response.Content))
            {
                var jsonResponse = response.Content;
                var cotizacionResponse = JsonSerializer.Deserialize<CotizacionActivoResponse>(jsonResponse);
                
                cotizaciones = cotizacionResponse!.titulos;
            }
        }

        if (cotizaciones != null)
        {
            cotizaciones = cotizaciones.OrderBy(b => b.variacionPorcentual).ToList(); 
        }
        else
        {
            Console.WriteLine("La deserialización devolvió un objeto nulo.");
        }

        return cotizaciones!;
    }

    public async Task<TituloDetalle> GetCotizacionesDetalleAsync(string mercado, string simbolo, string accessToken)
    {
        var url = $"/api/v2/{mercado}/Titulos/{simbolo}/CotizacionDetalle";
        var client = new RestClient("https://api.invertironline.com");
        var request = new RestRequest(url, Method.Get);
        request.AddHeader("Authorization", $"Bearer {accessToken}");

        var response = await client.ExecuteAsync(request);
        var cotizacionDetalle = new TituloDetalle();

        if (response.IsSuccessful)
        {
            if (!string.IsNullOrEmpty(response.Content))
            {
                var jsonResponse = response.Content;
                var cotizacionResponse = JsonSerializer.Deserialize<TituloDetalle>(jsonResponse);
                
                cotizacionDetalle = cotizacionResponse;
            }
        }

        if (cotizacionDetalle == null)
        {
            Console.WriteLine("La deserialización devolvió un objeto nulo.");
        }

        return cotizacionDetalle!;
    }

    public async Task<List<Operacion>> GetOperacionesFiltradasAsync(string? estado, string? tipo, DateTime? fechaDesde, DateTime? fechaHasta, string? accessToken)
    {
        string fechaDesdeStr = fechaDesde.HasValue ? fechaDesde.Value.ToString("yyyy-MM-dd") : string.Empty;
        string fechaHastaStr = fechaHasta.HasValue ? fechaHasta.Value.ToString("yyyy-MM-dd") : string.Empty;

        var url = $"/api/v2/operaciones?filtro.estado={estado}&filtro.fechaDesde={fechaDesdeStr}&filtro.fechaHasta={fechaHastaStr}";
        var client = new RestClient("https://api.invertironline.com");
        var request = new RestRequest(url, Method.Get);
        request.AddHeader("Authorization", $"Bearer {accessToken}");

        var response = await client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            if (!string.IsNullOrEmpty(response.Content))
            {
                try
                {
                    var operacionesFiltradas = JsonSerializer.Deserialize<List<Operacion>>(response.Content) ?? new List<Operacion>();

                    if (!string.IsNullOrEmpty(tipo) && tipo != "todas")
                    {
                        operacionesFiltradas = operacionesFiltradas.Where(o => o.tipo == tipo).ToList();
                    }

                    return operacionesFiltradas;
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Error al deserializar la respuesta: {ex.Message}");
                    return new List<Operacion>();
                }
            }
            else
            {
                Console.WriteLine("La respuesta del servidor está vacía.");
                return new List<Operacion>();
            }
        }
        else
        {
            Console.WriteLine($"Error al obtener las operaciones. Código de estado: {response.StatusCode}, Mensaje: {response.ErrorMessage}, Contenido: {response.Content}");
            return new List<Operacion>();
        }
    }

}