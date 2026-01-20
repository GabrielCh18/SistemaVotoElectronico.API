using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVotoElectronico.API.Data;

namespace SistemaVotoElectronico.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetupController : ControllerBase
    {
        private readonly VotoContext _context;

        public SetupController(VotoContext context)
        {
            _context = context;
        }

        // POST: api/Setup/crear-datos-prueba
        [HttpPost("crear-datos-prueba")]
        public async Task<IActionResult> CrearDatosPrueba()
        {
            // 1. SQL para crear la Estructura Geográfica (Junta #1)
            var sqlJunta = @"
                INSERT INTO ""Provincias"" (""Id"", ""Nombre"") OVERRIDING SYSTEM VALUE VALUES (1, 'Imbabura') ON CONFLICT DO NOTHING;
                INSERT INTO ""Cantones"" (""Id"", ""Nombre"", ""ProvinciaId"") OVERRIDING SYSTEM VALUE VALUES (1, 'Ibarra', 1) ON CONFLICT DO NOTHING;
                INSERT INTO ""Parroquias"" (""Id"", ""Nombre"", ""CantonId"") OVERRIDING SYSTEM VALUE VALUES (1, 'San Francisco', 1) ON CONFLICT DO NOTHING;
                INSERT INTO ""Zonas"" (""Id"", ""Nombre"", ""Direccion"", ""ParroquiaId"") OVERRIDING SYSTEM VALUE VALUES (1, 'Centro', 'Calle Bolivar', 1) ON CONFLICT DO NOTHING;
                INSERT INTO ""Juntas"" (""Id"", ""Numero"", ""Genero"", ""ZonaId"") OVERRIDING SYSTEM VALUE VALUES (1, 1, 'M', 1) ON CONFLICT DO NOTHING;
            ";

            // 2. SQL para crear al Votante 'Andres Test' y su Token
            var sqlVotante = @"
                INSERT INTO ""Votantes"" (""Nombre"", ""Apellido"", ""Cedula"", ""YaVoto"", ""JuntaId"")
                VALUES ('Andres', 'Test', '9999999999', FALSE, 1);

                INSERT INTO ""Tokens"" (""VotanteId"", ""CodigoUnico"", ""FechaExpiracion"", ""FueUsado"")
                VALUES (
                    (SELECT ""Id"" FROM ""Votantes"" WHERE ""Cedula"" = '9999999999' LIMIT 1),
                    '12345', 
                    '2030-12-31', 
                    FALSE
                );
            ";

            try
            {
                // Ejecutamos los comandos directos en la base
                await _context.Database.ExecuteSqlRawAsync(sqlJunta);
                await _context.Database.ExecuteSqlRawAsync(sqlVotante);
                return Ok("✅ ¡Datos inyectados con éxito! Ya tienes Junta #1 y Votante '9999999999'.");
            }
            catch (Exception ex)
            {
                return BadRequest($"❌ Error: {ex.Message}");
            }
        }
    }
}