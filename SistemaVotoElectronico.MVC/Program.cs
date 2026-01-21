using SistemaVotoElectronico.ApiConsumer; // <--- Referencia a tu servicio
using System.Net.Http; // <--- Necesario para el manejo de certificados

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// -----------------------------------------------------------------------------
// 🔌 CONEXIÓN CON LA API (MODO DESARROLLO)
// -----------------------------------------------------------------------------
// Aquí inyectamos el ApiService, pero le agregamos una configuración especial
// para que confíe en el certificado de seguridad local (localhost) y no de error.
builder.Services.AddHttpClient<ApiService>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    });
// -----------------------------------------------------------------------------

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();