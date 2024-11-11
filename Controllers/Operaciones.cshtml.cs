using Microsoft.AspNetCore.Mvc.RazorPages;
using RestSharp;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using InvertirOnlineApp.Models; 

namespace InvertirOnlineApp.Pages 
{
    public class OperacionesModel : PageModel
    {
        public string? OperacionesData { get; set; }
        public string? AccessToken { get; set; }
        public string? FiltroEstado { get; set; }
        public string? FiltroTipo{ get; set; }
        public DateTime? FiltroFechaDesde { get; set; }
        public DateTime? FiltroFechaHasta { get; set; }
        public List<Operacion> Operaciones { get; set; } = new List<Operacion>();

        public async Task OnGetAsync(string? filtroEstado, string? filtroTipo, DateTime? filtroFechaDesde, DateTime? filtroFechaHasta)
        {
            var tokenJson = HttpContext.Session.GetString("AuthToken");
            if (!string.IsNullOrEmpty(tokenJson))
            {
                var tokenObject = JsonSerializer.Deserialize<TokenResponse>(tokenJson);
                AccessToken = tokenObject?.AccessToken;

                var accessToken = tokenObject?.AccessToken;
                Console.WriteLine($"Token: {accessToken}");

                if (!string.IsNullOrEmpty(accessToken))
                {
                    // Si hay filtros, llamar a la API filtrada
                    if (!string.IsNullOrEmpty(filtroEstado) || !string.IsNullOrEmpty(filtroTipo) || filtroFechaDesde.HasValue || filtroFechaHasta.HasValue)
                    {
                        Operaciones = await ObtenerOperacionesFiltradas(filtroEstado, filtroTipo, filtroFechaDesde, filtroFechaHasta, accessToken);
                    }
                    else // Si no hay filtros, obtener todas las operaciones
                    {
                        var client = new RestClient("https://api.invertironline.com");
                        var request = new RestRequest("/api/v2/operaciones", Method.Get);
                        request.AddHeader("Authorization", $"Bearer {accessToken}");

                        var response = await client.ExecuteAsync(request);

                        if (response.IsSuccessful)
                        {
                            OperacionesData = response.Content; 

                            if (!string.IsNullOrEmpty(response.Content))
                            {
                                try
                                {
                                    Operaciones = JsonSerializer.Deserialize<List<Operacion>>(OperacionesData!) ?? new List<Operacion>();
                                }
                                catch (JsonException ex)
                                {
                                    OperacionesData = $"Error al deserializar la respuesta: {ex.Message}";
                                    Console.WriteLine(OperacionesData);
                                }
                            }
                            else
                            {
                                OperacionesData = "La respuesta del servidor está vacía.";
                                Console.WriteLine(OperacionesData);
                            }
                        }
                        else
                        {
                            OperacionesData = $"Error al obtener las operaciones. Código de estado: {response.StatusCode}, Mensaje: {response.ErrorMessage}, Contenido: {response.Content}";
                            Console.WriteLine(OperacionesData);
                        }
                    }
                }
            }
        }

        private async Task<List<Operacion>> ObtenerOperacionesFiltradas(string? estado, string? tipo, DateTime? fechaDesde, DateTime? fechaHasta, string? accessToken)
        {
            using (var httpClient = new HttpClient())
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
                    OperacionesData = response.Content;

                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        try
                        {
                            var operacionesFiltradas = JsonSerializer.Deserialize<List<Operacion>>(OperacionesData!) ?? new List<Operacion>();

                            if (!string.IsNullOrEmpty(tipo) && tipo != "todas")
                            {
                                operacionesFiltradas = operacionesFiltradas.Where(o => o.tipo == tipo).ToList();
                            }

                            return operacionesFiltradas;
                        }
                        catch (JsonException ex)
                        {
                            OperacionesData = $"Error al deserializar la respuesta: {ex.Message}";
                            Console.WriteLine(OperacionesData);
                            return new List<Operacion>();
                        }
                    }
                    else
                    {
                        OperacionesData = "La respuesta del servidor está vacía.";
                        Console.WriteLine(OperacionesData);
                        return new List<Operacion>();
                    }
                }
                else
                {
                    OperacionesData = $"Error al obtener las operaciones. Código de estado: {response.StatusCode}, Mensaje: {response.ErrorMessage}, Contenido: {response.Content}";
                    Console.WriteLine(OperacionesData);
                    return new List<Operacion>();
                }

            }
        }

        // Clase para deserializar el JSON del token
        public class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }
        }
    }
}