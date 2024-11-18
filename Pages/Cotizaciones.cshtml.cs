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
                    RiesgoPais = await _economiaService.GetRiesgoPaisAsync();
                    VariablesEconomicas = await _economiaService.GetVariablesEconomicasAsync();
                    if(VariablesEconomicas.Count() > 0){
                        InflacionMensual = VariablesEconomicas.FirstOrDefault(ve => ve.idVariable == 27)?.valor;
                        TasaReferencia = VariablesEconomicas.FirstOrDefault(ve => ve.idVariable == 6)?.valor;
                    }
                }       
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
    }
}
