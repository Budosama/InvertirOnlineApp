using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using RestSharp;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using InvertirOnlineApp.Models;

namespace InvertirOnlineApp.Pages 
{
    public class MiPortafolioModel : PageModel
    {
        private readonly CurrencyService _currencyService;
        private readonly BancoInfoService _bancoInfoService;
        private readonly EconomiaService _economiaService;
        private readonly IolService _iolService;
        public MiPortafolioModel(CurrencyService currencyService, BancoInfoService bancoInfoService, EconomiaService economiaService, IolService iolService)
        {
            _currencyService = currencyService;
            _bancoInfoService = bancoInfoService;
            _economiaService = economiaService;
            _iolService = iolService;
        }

        public string? PortafolioData { get; set; }
        public string? AccessToken { get; set; }
        public string? SimboloSeleccionado { get; set; }
        public List<Activo> PortafolioItems { get; set; } = new List<Activo>(); 
        public List<BancoInfo> Bancos { get; set; } = new List<BancoInfo>(); 
        public List<VariableEconomica> VariablesEconomicas { get; set; } = new List<VariableEconomica>(); 
        public List<Operacion> Operaciones { get; set; } = new List<Operacion>();

        [BindProperty]
        public decimal? Tna { get; set; }
        public decimal? TnaPorMesResult { get; set; }  
        public decimal? TnaPorDiaResult { get; set; }  
        public decimal? InflacionMensual { get; set; }
        public decimal? InflacionAnual { get; set; }  
        public decimal? InflacionAnualEsperada { get; set; } 
        public decimal? RiesgoPais { get; set; } 
        public decimal? Btc { get; set; }  
        public decimal? Usd { get; set; } 

        public decimal CalcularPorcentajePlazoFijoCompuesto(int dias)
        {
            decimal? tasaDecimal = Tna / 100;
            decimal tiempoEnAnios = dias / 365.0m; 
            int n = 365;
            decimal porcentajeCompuesto = (decimal)Math.Pow((double)(1 + tasaDecimal! / n), n * (double)tiempoEnAnios) - 1;
            return porcentajeCompuesto * 100;
        }

        public decimal CalcularValorizadoPrevio()
        {
            decimal valorizadoPrevio = 0;
            foreach (var activo in PortafolioItems)
            {
                if (activo.variacionDiaria > 0)
                {
                    valorizadoPrevio += activo.cantidad * (activo.ultimoPrecio / (1 + (activo.variacionDiaria / 100)));                   
                } 
                else if (activo.variacionDiaria < 0)
                {
                    valorizadoPrevio += activo.cantidad * (activo.ultimoPrecio / (1 - (activo.variacionDiaria / 100)));
                } 
                else 
                {
                    valorizadoPrevio += activo.cantidad * activo.ultimoPrecio;
                }
            }
            return valorizadoPrevio;
        }

        public decimal? CalcularGananciaNetaPrevia(decimal? totalInvertido, decimal? valorizadoPrevio)
        {
            decimal? gananciaNetaPrevia;
            decimal? ganancia = valorizadoPrevio - totalInvertido;
            gananciaNetaPrevia = ganancia * 100 / totalInvertido;
            return gananciaNetaPrevia;
        }

        public decimal? CalcularGananciaNetaPreviaEnPesos(decimal? totalInvertido, decimal? valorizadoPrevio)
        {
            decimal? gananciaNetaPrevia = valorizadoPrevio - totalInvertido;
            return gananciaNetaPrevia;
        }

        public string ObtenerFechaVencimiento(string? leacap)
        {            
            if (leacap!.Length != 5)
            {
                throw new ArgumentException("Formato de LECAP no válido");
            }
            try
            {
                int dia = int.Parse(leacap.Substring(1, 2));
                int anioBase = 2020; 
                int anio = anioBase + int.Parse(leacap.Substring(4, 1));
                string fechaVencimiento = $"LECAP-VTO-{dia:00}-01-{anio}";
                return fechaVencimiento;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error al procesar la fecha de vencimiento: " + ex.Message);
            }
        }

        public async Task<IActionResult> OnGetCurrencyValues()
        {
            if(!Btc.HasValue){
                Btc = await _currencyService.GetBTCValueInUSDAsync(); 
            }
            if(!Usd.HasValue){
                Usd = await _currencyService.GetUSDTValueInARSAsync(); 
            }
            if(!(Bancos.Count > 0)){
                Bancos = await _bancoInfoService.GetBancoInfoAsync();
                if(!Tna.HasValue){
                    Tna = (decimal?)Bancos.Max(b => b.tnaClientes) * 100;        
                }         
            }
            if(!(VariablesEconomicas.Count > 0)){
                VariablesEconomicas = await _economiaService.GetVariablesEconomicasAsync();    
                InflacionMensual = VariablesEconomicas.FirstOrDefault(ve => ve.idVariable == 27)?.valor;
                InflacionAnual = VariablesEconomicas.FirstOrDefault(ve => ve.idVariable == 28)?.valor;
                InflacionAnualEsperada = VariablesEconomicas.FirstOrDefault(ve => ve.idVariable == 29)?.valor;
            }            
            if (Tna.HasValue && Tna.Value >= 0)
            {
                TnaPorMesResult = Tna.Value / 12;
                TnaPorDiaResult = TnaPorMesResult / 30;
            }
            if(!RiesgoPais.HasValue){
                RiesgoPais = await _economiaService.GetRiesgoPaisAsync();    
            } 

            return new JsonResult(new { btc = Btc, usd = Usd }); 
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }

            await OnGetAsync();
            return Page();
        }

        public IActionResult OnPostActualizarSimbolo([FromBody] JsonElement data)
        {
            
            if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("simbolo", out JsonElement simboloElement))
            {
                string simbolo = simboloElement.GetString()!;
                if (string.IsNullOrEmpty(simbolo))
                {
                    return BadRequest(new { success = false, message = "El símbolo es nulo o vacío" });
                }

                SimboloSeleccionado = simbolo;
                return new JsonResult(new { success = true });
            }
            else
            {
                return BadRequest(new { success = false, message = "El cuerpo de la solicitud no contiene la propiedad 'simbolo'" });
            }
        }

        public async Task OnGetAsync()
        {           
            await OnGetCurrencyValues();

            var tokenJson = HttpContext.Session.GetString("AuthToken"); 
            if (!string.IsNullOrEmpty(tokenJson))
            {
                var tokenObject = JsonSerializer.Deserialize<TokenResponse>(tokenJson); 
                AccessToken = tokenObject?.AccessToken!;

                var accessToken = tokenObject?.AccessToken; 

                if (!string.IsNullOrEmpty(accessToken))
                {
                    var client = new RestClient("https://api.invertironline.com");
                    var request = new RestRequest("/api/v2/portafolio/argentina", Method.Get);
                    request.AddHeader("Authorization", $"Bearer {accessToken}"); 

                    var response = await client.ExecuteAsync(request);

                    if (response.IsSuccessful)
                    {                   
                        PortafolioData = response.Content; 

                        if (!string.IsNullOrEmpty(response.Content))
                        {
                            try
                            {
                                var portafolioResponse = JsonSerializer.Deserialize<PortafolioResponse>(response.Content);
                                
                                if (portafolioResponse != null)
                                {
                                    PortafolioItems = portafolioResponse.activos;
                                    if (PortafolioItems != null && PortafolioItems.Any())
                                    {
                                        DateTime fechaDesde = DateTime.Now.AddYears(-5);
                                        DateTime fechaHasta = DateTime.Now;
                                        Operaciones = await ObtenerOperacionesFiltradas("terminadas", "todas", fechaDesde, fechaHasta, accessToken);                                     

                                        var simbolosTipoEspecial = PortafolioItems
                                            .Where(item => item.titulo.tipo == "TitulosPublicos" || item.titulo.tipo == "Letras")
                                            .Select(item => item.titulo.simbolo)
                                            .ToHashSet();

                                        // Recorrer las operaciones y dividir precioOperado si el símbolo pertenece al conjunto
                                        foreach (var operacion in Operaciones)
                                        {
                                            if (simbolosTipoEspecial.Contains(operacion.simbolo!))
                                            {
                                                operacion.precioOperado /= 100;
                                            }
                                        }

                                        var comprasPorSimbolo = Operaciones
                                            .Where(o => o.tipo == "Compra")
                                            .GroupBy(o => o.simbolo)
                                            .Select(g => new
                                            {
                                                simbolo = g.Key,
                                                fechaPrimerCompra = g.Min(o => o.fechaOperada),
                                                sumatoriaMontoCompras = g.Sum(o => o.montoOperado)
                                            }).ToList();

                                        foreach (var item in PortafolioItems)
                                        {
                                            var compra = comprasPorSimbolo.FirstOrDefault(c => c.simbolo == item.titulo.simbolo);
                                            if (compra != null)
                                            {
                                                item.fechaPrimerCompra = compra.fechaPrimerCompra;
                                                // item.sumatoriaMontoCompras = compra.sumatoriaMontoCompras; // Esto no es correcto
                                            }

                                            if (item.titulo.tipo == "TitulosPublicos")
                                            {
                                                item.titulo.tipo = "BONOS";
                                                item.ppc /= 100;
                                                item.ultimoPrecio /= 100;
                                            } 
                                            else if (item.titulo.tipo == "Letras")
                                            {
                                                item.titulo.tipo = "LEACAPS";
                                                item.ppc /= 100;
                                                item.ultimoPrecio /= 100;
                                            }
                                            else if (item.titulo.tipo == "FondoComundeInversion")
                                            {
                                                item.titulo.tipo = "FCI";
                                            }
                                        }
                                        
                                    }
                                    else
                                    {
                                        Console.WriteLine("No hay activos en el portafolio.");
                                    }
                                }
                                else
                                {
                                    PortafolioData = "portafolioResponse es null";
                                }
                            }
                            catch (JsonException ex)
                            {
                                PortafolioData = $"Error al deserializar la respuesta: {ex.Message}";
                            }
                        }
                        else
                        {
                            PortafolioData = "La respuesta del servidor está vacía.";
                        }
                    }
                    else
                    {
                        PortafolioData = $"Error al obtener el portafolio. Código de estado: {response.StatusCode}, Mensaje: {response.ErrorMessage}, Contenido: {response.Content}";
                    }
                }
            }
        }

        private async Task<List<Operacion>> ObtenerOperacionesFiltradas(string? estado, string? tipo, DateTime? fechaDesde, DateTime? fechaHasta, string? accessToken)
        {
            return await _iolService.GetOperacionesFiltradasAsync(estado, tipo, fechaDesde, fechaHasta, accessToken);
        }        

        public class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }
        }
    }
}
