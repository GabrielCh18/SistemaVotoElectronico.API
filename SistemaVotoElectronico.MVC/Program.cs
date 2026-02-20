using SistemaVotoElectronico.ApiConsumer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaVotoElectronico.MVC.Data;

var builder = WebApplication.CreateBuilder(args);

// CONFIGURACIÓN DE BASE DE DATOS (POSTGRESQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<SistemaVotoElectronicoMVCContext>(options =>
    options.UseNpgsql(connectionString));

// CONFIGURACIÓN DE IDENTITY (SEGURIDAD)
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<SistemaVotoElectronicoMVCContext>();

// MVC y SESIÓN
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// CLIENTE API (Conectado a tu API Local para la defensa)
builder.Services.AddHttpClient<ApiService>(client =>
{
    // AQUI ESTA LA MAGIA: Apuntando a tu API local
    string urlFija = "https://localhost:7090/api/";

    Console.WriteLine($"==================================================");
    Console.WriteLine($"🚀 CONECTANDO A API LOCAL: {urlFija}");
    Console.WriteLine($"==================================================");

    client.BaseAddress = new Uri(urlFija);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
});

var app = builder.Build();

// PIPELINE HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// SEGURIDAD
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// RUTAS
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// INICIO DEL BLOQUE MÁGICO: Crear Admin Automáticamente 
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        // Crear el Rol "Admin" si no existe
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Buscar tu usuario y darle el poder
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

app.Run();