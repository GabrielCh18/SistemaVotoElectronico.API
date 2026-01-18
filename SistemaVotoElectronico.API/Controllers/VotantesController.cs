using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVotoElectronico.API.Data;
using SistemaVoto.Modelos;

namespace SistemaVotoElectronico.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VotantesController : ControllerBase
    {
        private readonly VotoContext _context;

        public VotantesController(VotoContext context)
        {
            _context = context;
        }

        // 1. BUSCAR POR CÉDULA: Lo usa el Jefe de Junta al recibir al ciudadano
        [HttpGet("buscar/{cedula}")]
        public async Task<ActionResult<Votante>> GetVotante(string cedula)
        {
            var votante = await _context.Votantes
                .Include(v => v.Junta)
                .FirstOrDefaultAsync(v => v.Cedula == cedula);

            if (votante == null) return NotFound("Ciudadano no encontrado.");
            return Ok(votante);
        }

        // 2. GENERAR CÓDIGO ÚNICO: El Jefe de Junta valida presencia y genera el token
        [HttpPost("generar-acceso/{votanteId}")]
        public async Task<IActionResult> GenerarAcceso(int votanteId)
        {
            var votante = await _context.Votantes.FindAsync(votanteId);
            if (votante == null) return NotFound("Votante no existe.");
            if (votante.YaVoto) return BadRequest("El ciudadano ya votó.");

            // Generar un código aleatorio de 6 dígitos
            string codigo = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

            var token = new TokenVotacion
            {
                VotanteId = votanteId,
                CodigoUnico = codigo,
                FechaExpiracion = DateTime.Now.AddMinutes(15), // Válido por 15 minutos
                FueUsado = false
            };

            _context.Tokens.Add(token);
            await _context.SaveChangesAsync();

            return Ok(new { codigoParaElVotante = codigo });
        }
    }
}