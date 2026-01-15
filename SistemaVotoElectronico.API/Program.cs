using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

// 1. Importar el namespace correcto de tu carpeta Data
using SistemaVotoElectronico.API.Data;

// 2. Importar tus Modelos
using SistemaVoto.Modelos;

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURACIÓN DE BASE DE DATOS ---
// Asegúrate de que en appsettings.json la cadena se llame "VotoElectronicoConnection"
var connectionString = builder.Configuration.GetConnectionString("VotoElectronicoConnection");

// CORRECCIÓN: Aquí usamos 'VotoContext', que es el nombre real de tu clase en la carpeta Data
builder.Services.AddDbContext<VotoContext>(options =>
    options.UseNpgsql(connectionString));

// --- CONFIGURACIÓN JWT ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("EstaEsMiClaveSuperSecretaDe123456")),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- CONFIGURACIÓN SWAGGER ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sistema Voto Electrónico API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese 'Bearer' [espacio] y luego su token.\r\n\r\nEjemplo: \"Bearer 12345abcdef\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// --- PIPELINE DE LA APLICACIÓN ---

app.UseCors();

// Habilitar Swagger siempre
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Validación de conexión al arrancar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // CORRECCIÓN: Aquí también usamos 'VotoContext'
        var context = services.GetRequiredService<VotoContext>();

        // Opcional: Esto crea la base de datos si no existe
        // context.Database.Migrate(); 
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al conectar con la base de datos.");
    }
}

app.Run();