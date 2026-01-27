using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVotoElectronico.API.Data;
using SistemaVoto.Modelos;

namespace SistemaVotoElectronico.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VotosController : ControllerBase
    {
        private readonly VotoContext _context;

        public VotosController(VotoContext context)
        {
            _context = context;
        }

        // --- CORRECCIÓN AQUÍ ---
        // Agregamos 'int procesoElectoralId' para saber a qué elección pertenece el voto
        [HttpPost("registrar-voto")]
        public async Task<IActionResult> RegistrarVoto(string cedula, string codigo, int candidatoId, int procesoElectoralId)
        {
            // 1. Validaciones de Token
            var token = await _context.Tokens
                .FirstOrDefaultAsync(t => t.CodigoUnico == codigo && !t.FueUsado);

            if (token == null) return BadRequest("Código inválido o ya usado.");
            if (token.FechaExpiracion < DateTime.UtcNow) return BadRequest("El código ha expirado.");

            // 2. Validaciones de Votante
            var votante = await _context.Votantes.FirstOrDefaultAsync(v => v.Cedula == cedula);

            // Verificamos que el votante exista y que el token sea SUYO
            if (votante == null) return BadRequest("Ciudadano no encontrado.");
            if (token.VotanteId != votante.Id) return BadRequest("Este código no pertenece a esta cédula.");
            if (votante.YaVoto) return BadRequest("El ciudadano ya votó.");

            // 3. Crear el voto
            // ... dentro del método de guardar voto ...

            var nuevoVoto = new Voto
            {
                IdVotante = votante.Id,
                CandidatoId = candidatoId,
                ProcesoElectoralId = procesoElectoralId,

                // 👇 ASEGÚRATE QUE DIGA ESTO:
                FechaVoto = DateTime.Now
            };

            // 4. Actualizar estados
            votante.YaVoto = true;
            token.FueUsado = true;

            _context.Votos.Add(nuevoVoto);
            await _context.SaveChangesAsync();

            return Ok("✅ Voto registrado con éxito.");
        }
    }
}