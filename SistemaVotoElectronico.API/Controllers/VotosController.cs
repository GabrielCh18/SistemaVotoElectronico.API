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

        [HttpPost("registrar-voto")]
        public async Task<IActionResult> RegistrarVoto(
            string cedula,
            string codigo,
            int candidatoId,
            int procesoElectoralId)
        {
            var ahora = DateTime.UtcNow;

            // 1️⃣ Validar proceso electoral activo y dentro del horario
            var proceso = await _context.ProcesoElectorales
                .Include(p => p.Candidatos)
                .FirstOrDefaultAsync(p =>
                    p.Id == procesoElectoralId &&
                    p.Activo &&
                    p.FechaInicio <= ahora &&
                    p.FechaFin >= ahora
                );

            if (proceso == null)
                return BadRequest("No existe un proceso electoral activo o está fuera del horario permitido.");

            // 2️⃣ Validar que el candidato pertenezca al proceso
            bool candidatoValido = proceso.Candidatos
                .Any(c => c.Id == candidatoId);

            if (!candidatoValido)
                return BadRequest("El candidato no pertenece a este proceso electoral.");

            // 3️⃣ Validar token
            var token = await _context.Tokens
                .FirstOrDefaultAsync(t =>
                    t.CodigoUnico == codigo &&
                    !t.FueUsado &&
                    t.FechaExpiracion >= ahora
                );

            if (token == null)
                return BadRequest("Código inválido, usado o expirado.");

            // 4️⃣ Validar votante
            var votante = await _context.Votantes
                .FirstOrDefaultAsync(v => v.Cedula == cedula);

            if (votante == null)
                return BadRequest("Ciudadano no encontrado.");

            if (token.VotanteId != votante.Id)
                return BadRequest("Este código no pertenece a esta cédula.");

            // 5️⃣ Verificar si ya votó en ESTE proceso electoral
            bool yaVotoEnProceso = await _context.Votos.AnyAsync(v =>
                v.IdVotante == votante.Id &&
                v.ProcesoElectoralId == procesoElectoralId
            );

            if (yaVotoEnProceso)
                return BadRequest("El ciudadano ya votó en este proceso electoral.");

            // 6️⃣ Registrar el voto
            var nuevoVoto = new Voto
            {
                IdVotante = votante.Id,
                CandidatoId = candidatoId,
                ProcesoElectoralId = procesoElectoralId,
                FechaVoto = ahora
            };

            token.FueUsado = true;

            _context.Votos.Add(nuevoVoto);
            await _context.SaveChangesAsync();

            return Ok("✅ Voto registrado con éxito.");
        }
    }
}