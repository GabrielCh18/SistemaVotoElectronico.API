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

        // --- NUEVO: REGISTRAR VOTANTE DESDE SWAGGER ---
        [HttpPost("registrar")]
        public async Task<ActionResult<Votante>> RegistrarVotante(Votante votante)
        {
            // 1. Validar que la cédula no exista ya
            if (await _context.Votantes.AnyAsync(v => v.Cedula == votante.Cedula))
            {
                return BadRequest("Ya existe un votante con esa cédula.");
            }

            // 2. Validar que la Junta exista (Si mandas JuntaId = 999 y no existe, explota)
            var juntaExiste = await _context.Juntas.AnyAsync(j => j.Id == votante.JuntaId);
            if (!juntaExiste)
            {
                return BadRequest($"No existe la Junta con ID {votante.JuntaId}. Asegúrate de crear Juntas primero.");
            }

            // 3. Limpiamos datos por si acaso (para que no diga que ya votó al crearse)
            votante.YaVoto = false;
            votante.Junta = null; // Evitamos conflictos de referencia circular

            _context.Votantes.Add(votante);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVotante", new { cedula = votante.Cedula }, votante);
        }
        // ---------------------------------------------

        // 1. BUSCAR POR CÉDULA
        [HttpGet("buscar/{cedula}")]
        public async Task<ActionResult<Votante>> GetVotante(string cedula)
        {
            var votante = await _context.Votantes
                .Include(v => v.Junta)
                .ThenInclude(j => j.Zona) // Traemos también la Zona para saber dónde votar
                .FirstOrDefaultAsync(v => v.Cedula == cedula);

            if (votante == null) return NotFound("Ciudadano no encontrado.");
            return Ok(votante);
        }

        // 2. GENERAR CÓDIGO ÚNICO
        [HttpPost("generar-acceso/{votanteId}")]
        public async Task<IActionResult> GenerarAcceso(int votanteId)
        {
            var votante = await _context.Votantes.FindAsync(votanteId);
            if (votante == null) return NotFound("Votante no existe.");
            if (votante.YaVoto) return BadRequest("El ciudadano ya votó.");

            // Generar código de 6 dígitos
            string codigo = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

            var token = new TokenVotacion
            {
                VotanteId = votanteId,
                CodigoUnico = codigo,
                FechaExpiracion = DateTime.UtcNow.AddMinutes(15),
                FueUsado = false
            };

            _context.Tokens.Add(token);
            await _context.SaveChangesAsync();

            return Ok(new { codigoParaElVotante = codigo });
        }

        [HttpPost("generar-codigo/{cedula}")]
        public async Task<IActionResult> GenerarCodigoPorCedula(string cedula)
        {
            // 1. Buscamos por cédula
            var votante = await _context.Votantes.FirstOrDefaultAsync(v => v.Cedula == cedula);

            if (votante == null) return NotFound("Votante no existe.");
            if (votante.YaVoto) return BadRequest("El ciudadano ya votó.");

            // 2. Generamos código
            string codigo = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

            var token = new TokenVotacion
            {
                VotanteId = votante.Id,
                CodigoUnico = codigo,
                FechaExpiracion = DateTime.UtcNow.AddMinutes(15),
                FueUsado = false
            };

            _context.Tokens.Add(token);
            await _context.SaveChangesAsync();

            // 3. Retornamos el objeto JSON exacto que espera la clase RespuestaCodigo del MVC
            return Ok(new
            {
                codigo = codigo,
                nombre = $"{votante.Nombre} {votante.Apellido}"
            });
        }
    }
}