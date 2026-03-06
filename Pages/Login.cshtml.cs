using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using RestSharp;
using System.Threading.Tasks;
using System.Text.Json;

public class LoginModel : PageModel
{
    [BindProperty]
    public string? Username { get; set; }
    
    [BindProperty]
    public string? Password { get; set; }

    public string? Token { get; set; }

    public IActionResult OnGet()
    {
        ViewData["Title"] = "Login";

        // Verifica si el usuario ya está autenticado
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("AuthToken")))
        {
            return RedirectToPage("/Home"); // Redirige a la página principal si está autenticado
        }

        return Page(); // Muestra la página de login si no está autenticado
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Login por defecto para pruebas
        if (Username == "admin" && Password == "admin")
        {
            var fakeToken = new TokenResponse
            {
                AccessToken = "admin_fake_access_token",
                ExpiresIn = 3600,
                TokenType = "Bearer"
            };

            var tokenJson = JsonSerializer.Serialize(fakeToken);
            HttpContext.Session.SetString("AuthToken", tokenJson);
            HttpContext.Session.SetString("IsDemo", "true"); // opcional

            return RedirectToPage("/Home");
        }

        // Login real a la API
        var client = new RestClient("https://api.invertironline.com/token");
        var request = new RestRequest("/", Method.Post);
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddParameter("username", Username);
        request.AddParameter("password", Password);
        request.AddParameter("grant_type", "password");

        var response = await client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            Token = response.Content; // Aquí podés deserializar y obtener solo el access_token si querés
            HttpContext.Session.SetString("AuthToken", Token!);
            return RedirectToPage("/Home");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Error en la autenticación");
            return Page();
        }
    }


    public IActionResult OnGetLogout()
    {
        HttpContext.Session.Remove("AuthToken"); // Elimina el token de sesión
        return RedirectToPage("/Login"); // Redirige a la página de inicio de sesión
    }

}
