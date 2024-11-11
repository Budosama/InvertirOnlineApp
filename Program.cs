var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddRazorPages();
builder.Services.AddTransient<CurrencyService>();
builder.Services.AddTransient<BancoInfoService>();
builder.Services.AddTransient<EconomiaService>();

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession(); // Mover aquí es una buena práctica

app.Use(async (context, next) =>
{
    try
    {
        if (string.IsNullOrEmpty(context.Session.GetString("AuthToken")) && context.Request.Path != "/Login")
        {
            context.Response.Redirect("/Login");
            return;
        }
        await next();
    }
    catch (Exception ex)
    {
        // Log the error aquí si tienes un logger configurado
        Console.WriteLine($"Error en el middleware: {ex.Message}");
        throw; // Re-lanzar la excepción para que se maneje por DeveloperExceptionPage o middleware de manejo de errores.
    }
});

app.UseAuthorization();

app.MapRazorPages();
app.MapFallbackToPage("/Login");

app.Run();