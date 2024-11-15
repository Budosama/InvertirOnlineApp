using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using RestSharp;
using System.Threading.Tasks;

public class LoginModel : PageModel
{
    [BindProperty]
    public string? Username { get; set; }
    
    [BindProperty]
    public string? Password { get; set; }

    public string? Token { get; set; }

    public IActionResult OnGet()
    {
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

        var client = new RestClient("https://api.invertironline.com/token");
        var request = new RestRequest("/", Method.Post);
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddParameter("username", Username);
        request.AddParameter("password", Password);
        request.AddParameter("grant_type", "password");

        var response = await client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            Token = response.Content; // Puedes procesar la respuesta y extraer solo el token.
            HttpContext.Session.SetString("AuthToken", Token!); // Almacena el token en sesión.
            return RedirectToPage("/Home"); // Redirige a la página principal.
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
