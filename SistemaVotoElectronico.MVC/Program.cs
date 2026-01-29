using SistemaVotoElectronico.ApiConsumer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaVotoElectronico.MVC.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURACIÓN DE BASE DE DATOS (POSTGRESQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<SistemaVotoElectronicoMVCContext>(options =>
    options.UseNpgsql(connectionString));

// 2. CONFIGURACIÓN DE IDENTITY (SEGURIDAD)
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<SistemaVotoElectronicoMVCContext>();

// 3. MVC y SESIÓN
builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 4. CLIENTE API (Modo Fuerza Bruta: Solo Render)
builder.Services.AddHttpClient<ApiService>(client =>
{
    // OJO: Aquí estoy escribiendo la URL directamente para que no haya dudas.
    // Si esto funciona, es que tus archivos appsettings.json te están mintiendo.
    string urlFija = "https://sistemavotoelectronico-api-s0li.onrender.com/api/";

    Console.WriteLine($"==================================================");
    Console.WriteLine($"🚀 FORZANDO CONEXIÓN A: {urlFija}");
    Console.WriteLine($"==================================================");

    client.BaseAddress = new Uri(urlFija);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
});

var app = builder.Build();

// 5. PIPELINE HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 6. SEGURIDAD
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// 7. RUTAS
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// 🔥 INICIO DEL BLOQUE MÁGICO: Crear Admin Automáticamente 🔥
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        // 1. Crear el Rol "Admin" si no existe
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // 2. Buscar tu usuario y darle el poder
        // ⚠️ Correo configurado:
        string emailAdmin = "arevalodany16@gmail.com";

        var usuario = await userManager.FindByEmailAsync(emailAdmin);

        if (usuario != null)
        {
            // Si el usuario existe, le asignamos el rol
            if (!await userManager.IsInRoleAsync(usuario, "Admin"))
            {
                await userManager.AddToRoleAsync(usuario, "Admin");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error creando los roles automáticamente.");
    }
}
// 🔥 FIN DEL BLOQUE MÁGICO 🔥

app.Run();