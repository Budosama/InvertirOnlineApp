using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

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

        public List<DetalleMes> DetalleMesAMes { get; set; } = new List<DetalleMes>();

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
                    decimal capitalInicialMes = montoAcumulado;
                    decimal interesGanado = montoAcumulado * tasaMensual;
                    montoAcumulado += interesGanado + AgregadoMensual;

                    // Agregar el detalle de este mes a la lista
                    DetalleMesAMes.Add(new DetalleMes
                    {
                        Mes = i + 1,
                        CapitalInicial = capitalInicialMes,
                        InteresGanado = interesGanado,
                        AgregadoMensual = AgregadoMensual,
                        TotalAcumulado = montoAcumulado
                    });
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

    public class DetalleMes
    {
        public int Mes { get; set; }
        public decimal CapitalInicial { get; set; }
        public decimal InteresGanado { get; set; }
        public decimal AgregadoMensual { get; set; }
        public decimal TotalAcumulado { get; set; }
    }
}