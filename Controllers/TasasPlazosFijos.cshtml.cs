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
    public class TasasPlazosFijosModel : PageModel
    {
        private readonly BancoInfoService _bancoInfoService;
        public TasasPlazosFijosModel(BancoInfoService bancoInfoService)
        {
            _bancoInfoService = bancoInfoService;
        }

        public List<BancoInfo> Bancos { get; set; } = new List<BancoInfo>(); 

        public async Task OnGetAsync()
        {
            Bancos = await _bancoInfoService.GetBancoInfoAsync();
        }

    }
}
