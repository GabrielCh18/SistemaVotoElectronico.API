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
            // Validamos que el proceso exista
            var proceso = await _context.ProcesoElectorales.FindAsync(candidato.ProcesoElectoralId);
            if (proceso == null)
            {
                return BadRequest("El proceso electoral especificado no existe.");
            }

            _context.Candidatos.Add(candidato);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCandidatosPorProceso", new { procesoId = candidato.ProcesoElectoralId }, candidato);
        }
    }
}