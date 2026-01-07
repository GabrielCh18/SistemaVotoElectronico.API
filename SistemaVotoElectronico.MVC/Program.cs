using SistemaVotoElectronico.ApiConsumer; // <--- 1. AGREGAMOS ESTO

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 2. REGISTRAMOS EL SERVICIO PARA QUE EL MVC PUEDA HABLAR CON LA API
builder.Services.AddHttpClient<ApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Importante para que funcionen tus estilos y scripts
app.MapStaticAssets();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();