var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor
builder.Services.AddHttpClient();
builder.Services.AddRazorPages();
builder.Services.AddTransient<CurrencyService>();
builder.Services.AddTransient<BancoInfoService>();
builder.Services.AddTransient<EconomiaService>();

// Configuración de sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configuración del pipeline de la aplicación
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
app.UseSession();

// Middleware para redirigir al login si no hay token
app.Use(async (context, next) =>
{
    if (string.IsNullOrEmpty(context.Session.GetString("AuthToken")) && !context.Request.Path.StartsWithSegments("/Login"))
    {
        context.Response.Redirect("/Login");
        return;
    }
    await next();
});

// Autorizar y mapear endpoints
app.UseAuthorization();

app.MapRazorPages(); // Mapear las Razor Pages
app.MapFallbackToPage("/Login"); // Configurar la página por defecto como Login

app.Run();