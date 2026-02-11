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

        // REGISTRAR VOTANTE
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

        //  CAMBIO IMPORTANTE 1: BUSCAR VOTANTE Y SU ESTADO EN LA ELECCIÓN
        
        [HttpGet("buscar/{cedula}")]
        public async Task<ActionResult<Votante>> GetVotante(string cedula)
        {
            // 1. Buscamos al ciudadano
            var votante = await _context.Votantes
                .Include(v => v.Junta)
                .ThenInclude(j => j.Zona)
                .FirstOrDefaultAsync(v => v.Cedula == cedula);

            if (votante == null) return NotFound("Ciudadano no encontrado.");

            // 2. Buscamos si hay ELECCIÓN ACTIVA
            var ahora = DateTime.UtcNow;
            var procesoActivo = await _context.ProcesoElectorales
                .FirstOrDefaultAsync(p => p.Activo && p.FechaInicio <= ahora && p.FechaFin >= ahora);

            // Inicializamos en falso por defecto
            votante.YaVoto = false;
            votante.CertificadoDescargado = false;

            if (procesoActivo != null)
            {
                votante.NombreProceso = procesoActivo.Nombre;

                // CORRECCIÓN: Buscamos el VOTO específico en ESTE proceso
                var votoRegistrado = await _context.Votos
                    .FirstOrDefaultAsync(v => v.IdVotante == votante.Id &&
                                              v.ProcesoElectoralId == procesoActivo.Id);

                if (votoRegistrado != null)
                {
                    votante.YaVoto = true;
                    // Aquí leemos el estado real desde la tabla Votos, no del Votante
                    votante.CertificadoDescargado = votoRegistrado.CertificadoDescargado;
                }
            }

            // 3. RECUPERAMOS EL TOKEN ACTIVO (Si existe)
            var tokenActivo = await _context.Tokens
                .Where(t => t.VotanteId == votante.Id &&
                            !t.FueUsado &&
                            t.FechaExpiracion > DateTime.UtcNow)
                .OrderByDescending(t => t.FechaExpiracion)
                .FirstOrDefaultAsync();

            if (tokenActivo != null)
            {
                votante.Token = tokenActivo.CodigoUnico;
            }

            return Ok(votante);
        }

        // GENERAR CÓDIGO DE ACCESO POR ID
        [HttpPost("generar-acceso/{votanteId}")]
        public async Task<IActionResult> GenerarAcceso(int votanteId)
        {
            var votante = await _context.Votantes.FindAsync(votanteId);
            if (votante == null)
                return NotFound("Votante no existe.");

            string codigo = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

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

        // GENERAR CÓDIGO DE ACCESO POR CÉDULA
        [HttpPost("generar-codigo/{cedula}")]
        public async Task<IActionResult> GenerarCodigoPorCedula(string cedula)
        {
            var votante = await _context.Votantes.FirstOrDefaultAsync(v => v.Cedula == cedula);

            if (votante == null) return NotFound("Votante no existe.");

            string codigo = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

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

        // LISTAR TODOS LOS VOTANTES
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

        // CAMBIO IMPORTANTE 2: MARCAR EL VOTO, NO AL VOTANTE
        
        [HttpPost("marcar-certificado/{votanteId}")]
        public async Task<IActionResult> MarcarCertificado(int votanteId)
        {
            // 1. Buscamos el proceso electoral activo
            var procesoActivo = await _context.ProcesoElectorales
                .FirstOrDefaultAsync(p => p.Activo);

            if (procesoActivo == null)
                return NotFound("No hay ningún proceso electoral activo.");

            // 2. Buscamos el VOTO de este votante en ESTE proceso
            var voto = await _context.Votos
                .FirstOrDefaultAsync(v => v.IdVotante == votanteId &&
                                          v.ProcesoElectoralId == procesoActivo.Id);

            if (voto == null)
                return NotFound("No se encontró el voto. El ciudadano no ha sufragado en esta elección.");

            // 3. Marcamos el certificado como descargado EN EL VOTO
            voto.CertificadoDescargado = true;

            // Guardamos cambios en la tabla Votos
            _context.Entry(voto).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Certificado marcado exitosamente." });
        }
    }
}