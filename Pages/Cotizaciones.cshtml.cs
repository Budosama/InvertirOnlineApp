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

        public CotizacionesModel(IolService iolService)
        {
            _iolService = iolService;
        }

        public List<TituloV2> Cotizaciones { get; set; } = new List<TituloV2>(); 
        public List<string> Instrumentos { get; set; } = ["acciones","aDRs","cauciones","cedears","cHDP","futuros","letras","obligacionesNegociables","opciones","titulosPublicos"]; 
        public List<string> Paises { get; set; } = ["argentina","estados_Unidos"]; 

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
                }       
            }  
        }   

        public string ClasificarBono(TituloV2 activo)
        {
            if (activo.ultimoPrecio.HasValue && activo.ultimoCierre.HasValue)
            {
                if(activo.ultimoPrecio.Value > 0 && activo.ultimoCierre.Value > 0)
                {
                    decimal variacion = ((activo.ultimoPrecio.Value - activo.ultimoCierre.Value) / activo.ultimoCierre.Value) * 100;
                    if (variacion > 5.0m)  // Si el bono ha subido más del 5%, es buen momento para comprar
                    {
                        return "Comprar";
                    }
                    else if (variacion < -5.0m)  // Si ha bajado más del 5%, podría ser un bono a vender
                    {
                        return "Vender";
                    }
                    else  // Si está estable, mantenerlo podría ser una opción
                    {
                        return "Mantener";
                    }
                }         
            }
            return "Mantener";
        }
    }
}
