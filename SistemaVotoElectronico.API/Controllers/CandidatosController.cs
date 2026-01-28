using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVotoElectronico.API.Data;
using SistemaVoto.Modelos;

namespace SistemaVotoElectronico.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidatosController : ControllerBase
    {
        private readonly VotoContext _context;

        public CandidatosController(VotoContext context)
        {
            _context = context;
        }

        // GET: api/Candidatos/por-proceso/1
        // Este es vital: Trae solo los candidatos de una elección específica
        [HttpGet("por-proceso/{procesoId}")]
        public async Task<ActionResult<IEnumerable<Candidato>>> GetCandidatosPorProceso(int procesoId)
        {
            return await _context.Candidatos
                                 .Where(c => c.ProcesoElectoralId == procesoId)
                                 .ToListAsync();
        }

        // POST: api/Candidatos
        [HttpPost]
        public async Task<ActionResult<Candidato>> PostCandidato(Candidato candidato)
        {
            // 🔥 Buscar el proceso activo automáticamente
            var procesoActivo = await _context.ProcesoElectorales
                .Where(p => p.Activo)
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync();

            if (procesoActivo == null)
                return BadRequest("No hay un proceso electoral activo.");

            // 💥 FORZAMOS el proceso correcto
            candidato.ProcesoElectoralId = procesoActivo.Id;

            _context.Candidatos.Add(candidato);
            await _context.SaveChangesAsync();

            return Ok(candidato);
        }

        // DELETE: api/Candidatos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCandidato(int id)
        {
            var candidato = await _context.Candidatos.FindAsync(id);
            if (candidato == null)
            {
                return NotFound("El candidato no existe.");
            }

            // 1. BUSCAMOS LOS VOTOS DE ESTE CANDIDATO
            var votos = _context.Votos.Where(v => v.CandidatoId == id).ToList();

            // 2. LOS BORRAMOS PRIMERO (Limpieza)
            if (votos.Any())
            {
                _context.Votos.RemoveRange(votos);
            }

            // 3. AHORA SÍ, BORRAMOS AL CANDIDATO
            _context.Candidatos.Remove(candidato);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Si falla por otra cosa, avisamos
                return BadRequest("Error crítico al borrar: " + ex.InnerException?.Message ?? ex.Message);
            }

            return NoContent();
        }
    }
}