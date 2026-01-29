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

        // --------------------------------------------------
        // REGISTRAR VOTANTE
        // --------------------------------------------------
        [HttpPost("registrar")]
        public async Task<ActionResult<Votante>> RegistrarVotante(Votante votante)
        {
            if (await _context.Votantes.AnyAsync(v => v.Cedula == votante.Cedula))
                return BadRequest("Ya existe un votante con esa cédula.");

            var juntaExiste = await _context.Juntas.AnyAsync(j => j.Id == votante.JuntaId);
            if (!juntaExiste)
                return BadRequest($"No existe la Junta con ID {votante.JuntaId}.");

            // Evitar problemas de navegación
            votante.Junta = null;

            _context.Votantes.Add(votante);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVotante), new { cedula = votante.Cedula }, votante);
        }

        // --------------------------------------------------
        // BUSCAR VOTANTE POR CÉDULA
        // --------------------------------------------------
        [HttpGet("buscar/{cedula}")]
        public async Task<ActionResult<Votante>> GetVotante(string cedula)
        {
            // 1. Buscamos al ciudadano y sus datos
            var votante = await _context.Votantes
                .Include(v => v.Junta)
                .ThenInclude(j => j.Zona)
                .FirstOrDefaultAsync(v => v.Cedula == cedula);

            if (votante == null) return NotFound("Ciudadano no encontrado.");

            // 2. Buscamos si hay ELECCIÓN ACTIVA
            var ahora = DateTime.UtcNow;
            var procesoActivo = await _context.ProcesoElectorales
                .FirstOrDefaultAsync(p => p.Activo && p.FechaInicio <= ahora && p.FechaFin >= ahora);

            if (procesoActivo != null)
            {
                votante.NombreProceso = procesoActivo.Nombre;

                // Verificamos si ya votó en ESTE proceso
                votante.YaVoto = await _context.Votos.AnyAsync(v =>
                    v.IdVotante == votante.Id &&
                    v.ProcesoElectoralId == procesoActivo.Id
                );
            }

            // 3. 🔥 RECUPERAMOS EL TOKEN ACTIVO 🔥
            // Buscamos el código que no ha expirado y no se ha usado
            var tokenActivo = await _context.Tokens
                .Where(t => t.VotanteId == votante.Id &&
                            !t.FueUsado &&
                            t.FechaExpiracion > DateTime.UtcNow)
                .OrderByDescending(t => t.FechaExpiracion)
                .FirstOrDefaultAsync();

            if (tokenActivo != null)
            {
                votante.Token = tokenActivo.CodigoUnico; // Lo guardamos para enviarlo al MVC
            }

            return Ok(votante);
        }

        // --------------------------------------------------
        // GENERAR CÓDIGO DE ACCESO POR ID
        // --------------------------------------------------
        [HttpPost("generar-acceso/{votanteId}")]
        public async Task<IActionResult> GenerarAcceso(int votanteId)
        {
            var votante = await _context.Votantes.FindAsync(votanteId);
            if (votante == null)
                return NotFound("Votante no existe.");

            string codigo = Guid.NewGuid()
                .ToString("N")
                .Substring(0, 6)
                .ToUpper();

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

        // --------------------------------------------------
        // GENERAR CÓDIGO DE ACCESO POR CÉDULA
        // --------------------------------------------------
        [HttpPost("generar-codigo/{cedula}")]
        public async Task<IActionResult> GenerarCodigoPorCedula(string cedula)
        {
            var votante = await _context.Votantes
                .FirstOrDefaultAsync(v => v.Cedula == cedula);

            if (votante == null)
                return NotFound("Votante no existe.");

            string codigo = Guid.NewGuid()
                .ToString("N")
                .Substring(0, 6)
                .ToUpper();

            var token = new TokenVotacion
            {
                VotanteId = votante.Id,
                CodigoUnico = codigo,
                FechaExpiracion = DateTime.UtcNow.AddMinutes(15),
                FueUsado = false
            };

            _context.Tokens.Add(token);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                codigo = codigo,
                nombre = $"{votante.Nombre} {votante.Apellido}"
            });
        }
        // --------------------------------------------------
        // LISTAR TODOS LOS VOTANTES (Con ubicación completa)
        // --------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Votante>>> GetVotantes()
        {
            return await _context.Votantes
                .Include(v => v.Junta)
                    .ThenInclude(j => j.Zona)
                        .ThenInclude(z => z.Parroquia)
                            .ThenInclude(p => p.Canton)
                                .ThenInclude(c => c.Provincia)
                .ToListAsync();
        }

        [HttpPost("marcar-certificado/{id}")]
        public async Task<IActionResult> MarcarCertificado(int id)
        {
            var votante = await _context.Votantes.FindAsync(id);
            if (votante == null) return NotFound();

            votante.CertificadoDescargado = true;

            _context.Entry(votante).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}