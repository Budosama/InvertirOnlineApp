using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using InvertirOnlineApp.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using static InvertirOnlineApp.Pages.MiPortafolioModel;

namespace InvertirOnlineApp.Pages 
{
    public class BonosModel : PageModel
    {
        private readonly IolService _iolService;

        public BonosModel(IolService iolService)
        {
            _iolService = iolService;
        }

        public List<TituloV2> Bonos { get; set; } = new List<TituloV2>(); 

        public async Task OnGetAsync()
        {
            var tokenJson = HttpContext.Session.GetString("AuthToken"); 
            if (!string.IsNullOrEmpty(tokenJson))
            {
                var tokenObject = JsonSerializer.Deserialize<TokenResponse>(tokenJson); 
                var accessToken = tokenObject?.AccessToken; 
                if (!string.IsNullOrEmpty(accessToken))
                {
                    Bonos = await _iolService.GetCotizacionesAsync("titulosPublicos", "argentina", accessToken);
                }       
            }  
        }   
        public string ClasificarBono(TituloV2 bono)
        {
            // Verificar si el bono tiene datos suficientes
            if (bono.ultimoPrecio.HasValue && bono.ultimoCierre.HasValue)
            {
                if(bono.ultimoPrecio.Value > 0 && bono.ultimoCierre.Value > 0)
                {
                    // Calcular la variación porcentual
                    decimal variacion = ((bono.ultimoPrecio.Value - bono.ultimoCierre.Value) / bono.ultimoCierre.Value) * 100;

                    // Definir la lógica para clasificación
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

            // Si no tiene información suficiente, clasificar como "Mantener" por defecto
            return "Mantener";
        }
    }
}
