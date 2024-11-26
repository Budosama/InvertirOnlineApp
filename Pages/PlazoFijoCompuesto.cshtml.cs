using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InvertirOnlineApp.Pages 
{
    public class PlazoFijoCompuestoModel : PageModel
    {
        [BindProperty]
        public decimal CapitalInicial { get; set; }

        [BindProperty]
        public decimal Tna { get; set; }

        [BindProperty]
        public int PlazoMeses { get; set; }

        [BindProperty]
        public decimal AgregadoMensual { get; set; }

        public decimal? Resultado { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                // Calcular el monto total con interés compuesto
                decimal tasaMensual = Tna / 100 / 12; // TNA a tasa mensual
                decimal montoAcumulado = CapitalInicial;

                for (int i = 0; i < PlazoMeses; i++)
                {
                    montoAcumulado += montoAcumulado * tasaMensual; // Interés compuesto
                    montoAcumulado += AgregadoMensual; // Agregado mensual
                }

                Resultado = montoAcumulado;
                return Page(); // Regresar a la misma vista con los datos
            }
            else
            {
                return Page(); // Si hay errores de validación
            }
        }
    }
}