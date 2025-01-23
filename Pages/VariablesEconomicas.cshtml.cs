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
    public class VariablesEconomicasModel : PageModel
    {
        private readonly EconomiaService _economiaService;
        
        public VariablesEconomicasModel(EconomiaService economiaService)
        {
            _economiaService = economiaService;
        }
        public decimal? RiesgoPais;
        public List<VariableEconomica> VariablesEconomicas { get; set; } = new List<VariableEconomica>(); 

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "Variables Economicas";

            VariablesEconomicas = await _economiaService.GetVariablesEconomicasAsync();
        }

        public async Task ObtenerRiesgoPaisAsync()
        {
            RiesgoPais = await _economiaService.GetRiesgoPaisAsync();
        }

    }
}
