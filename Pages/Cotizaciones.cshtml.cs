using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using InvertirOnlineApp.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using static InvertirOnlineApp.Pages.MiPortafolioModel;

namespace InvertirOnlineApp.Pages 
{
    public class CotizacionesModel : PageModel
    {
        private readonly IolService _iolService;
        private readonly EconomiaService _economiaService;

        public CotizacionesModel(IolService iolService, EconomiaService economiaService)
        {
            _iolService = iolService;
            _economiaService = economiaService;
        }

        public List<TituloV2> Cotizaciones { get; set; } = new List<TituloV2>(); 
        public List<string> Instrumentos { get; set; } = ["acciones","aDRs","cauciones","cedears","cHDP","futuros","letras","obligacionesNegociables","opciones","titulosPublicos"]; 
        public List<string> Paises { get; set; } = ["argentina","estados_Unidos"]; 
        public List<VariableEconomica> VariablesEconomicas { get; set; } = new List<VariableEconomica>(); 
        public decimal? RiesgoPais { get; set; }
        public decimal? InflacionMensual { get; set; }
        public decimal? TasaReferencia { get; set; }
        public string InstrumentoSeleccionado { get; set; } = string.Empty;
        
        public async Task OnGetAsync(string instrumento = "acciones", string pais = "argentina")
        {
            var tokenJson = HttpContext.Session.GetString("AuthToken"); 
            if (!string.IsNullOrEmpty(tokenJson))
            {
                var tokenObject = JsonSerializer.Deserialize<TokenResponse>(tokenJson); 
                var accessToken = tokenObject?.AccessToken; 
                if (!string.IsNullOrEmpty(accessToken))
                {
                    Cotizaciones = await _iolService.GetCotizacionesAsync(instrumento, pais, accessToken);
                    InstrumentoSeleccionado = instrumento;

                    if(!RiesgoPais.HasValue){
                        RiesgoPais = await _economiaService.GetRiesgoPaisAsync();
                    }

                    if(VariablesEconomicas.Count() == 0) {
                        VariablesEconomicas = await _economiaService.GetVariablesEconomicasAsync();
                    } else {
                        InflacionMensual = VariablesEconomicas.FirstOrDefault(ve => ve.idVariable == 27)?.valor;
                        TasaReferencia = VariablesEconomicas.FirstOrDefault(ve => ve.idVariable == 6)?.valor;
                    }
                }       
            }  
        }   

        public async Task<IActionResult> OnGetDetalleAsync(string mercado, string simbolo)
        {
            var tokenJson = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(tokenJson))
            {
                return Unauthorized();
            }

            var tokenObject = JsonSerializer.Deserialize<TokenResponse>(tokenJson);
            var accessToken = tokenObject?.AccessToken;

            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized();
            }

            try
            {
                var detalle = await _iolService.GetCotizacionesDetalleAsync(mercado, simbolo, accessToken);
                if (detalle != null)
                {
                    return new JsonResult(detalle); // Devuelve el detalle del activo como JSON.
                }
                else
                {
                    return NotFound("No se encontró el detalle del activo.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener el detalle: {ex.Message}");
            }
        }

        public string ClasificarBono(TituloV2 bono)
        {
            if (bono.variacionPorcentual.HasValue)
            {
                decimal rendimientoDiario = bono.variacionPorcentual.Value; // Directamente el porcentaje diario
                decimal? rendimientoEsperado = (TasaReferencia / 12) + InflacionMensual + (RiesgoPais / 1000.0m);

                if (rendimientoDiario > rendimientoEsperado)
                {
                    return "Comprar";
                }
                else if (rendimientoDiario < -rendimientoEsperado)
                {
                    return "Vender";
                }
            }
            return "Mantener";
        }

        public string ObtenerFechaVencimiento(string? leacap)
        {            
            // if (leacap!.Length != 5)
            // {
            //     throw new ArgumentException("Formato de LECAP no válido");
            // }
            try
            {
                int dia = int.Parse(leacap!.Substring(1, 2));
                int anioBase = 2020; 
                int anio = anioBase + int.Parse(leacap.Substring(4, 1));
                string fechaVencimiento = $"LECAP-VTO-{dia:00}-01-{anio}";
                return fechaVencimiento;
            }
            catch (Exception)
            {
                // throw new ArgumentException("Error al procesar la fecha de vencimiento: " + ex.Message);
                return $"LECAP-VTO-DESCONOCIDO:00-01-DESCONOCIDO";
            }
        }
    }
}
