using SistemaVotoElectronico.ApiConsumer; // <--- Tu servicio API
using System.Net.Http; // <--- Para el certificado SSL

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar servicios al contenedor (MVC)
builder.Services.AddControllersWithViews();

// 2. Configurar la Sesión (Memoria para el Login de Admin)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de vida de la sesión
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3. Conexión con la API (Modo Desarrollo - Ignorar SSL)
builder.Services.AddHttpClient<ApiService>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    });

var app = builder.Build();

// 4. Configurar el pipeline de solicitudes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 5. ¡IMPORTANTE! Activar la Sesión (Debe ir antes de los controladores)
app.UseSession();

// 6. Configurar la ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();