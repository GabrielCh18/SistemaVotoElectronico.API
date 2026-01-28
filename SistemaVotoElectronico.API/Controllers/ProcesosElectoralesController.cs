using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVotoElectronico.API.Data;
using SistemaVoto.Modelos;

namespace SistemaVotoElectronico.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcesosElectoralesController : ControllerBase
    {
        private readonly VotoContext _context;

        public ProcesosElectoralesController(VotoContext context)
        {
            _context = context;
        }
        // GET: api/ProcesosElectorales/5
        // ESTE ES EL QUE FALTABA PARA QUE EL HISTORIAL FUNCIONE
        [HttpGet("{id}")]
        public async Task<ActionResult<ProcesoElectoral>> GetProceso(int id)
        {
            var proceso = await _context.ProcesoElectorales
                .Include(p => p.Candidatos) // Incluimos candidatos por si acaso
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proceso == null)
            {
                return NotFound();
            }

            return Ok(proceso);
        }
        // GET: todos los procesos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProcesoElectoral>>> GetProcesos()
        {
            return await _context.ProcesoElectorales
                .Include(p => p.Candidatos)
                .ToListAsync();
        }

        // GET: proceso activo y en horario
        [HttpGet("activo")]
        public async Task<ActionResult<ProcesoElectoral>> GetProcesoActivo()
        {
            // TRUCO PARA DOCKER/RENDER: 
            // Obtenemos la hora UTC (Universal) y le restamos 5 horas para simular Ecuador
            var ahoraEcuador = DateTime.UtcNow.AddHours(-5);

            var proceso = await _context.ProcesoElectorales
                .Include(p => p.Candidatos)
                .FirstOrDefaultAsync(p =>
                    p.Activo &&
                    p.FechaInicio <= ahoraEcuador && // Compara con hora Ecuador
                    p.FechaFin >= ahoraEcuador       // Compara con hora Ecuador
                );

            if (proceso == null)
                return NotFound("No existe un proceso electoral activo en este momento.");

            return Ok(proceso);
        }

        // POST: crear proceso electoral
        [HttpPost]
        public async Task<ActionResult> PostProceso(ProcesoElectoral proceso)
        {
            // Validación de fechas simple
            if (proceso.FechaInicio >= proceso.FechaFin)
            {
                return BadRequest("La fecha de inicio debe ser menor a la fecha de fin.");
            }

            // Aseguramos que se guarde como activo
            proceso.Activo = true;

            _context.ProcesoElectorales.Add(proceso);
            await _context.SaveChangesAsync();

            return Ok(proceso);
        }

        // ⚠️ CAMBIO IMPORTANTE: De PUT a POST para que funcione con tu MVC
        // POST: cerrar proceso manualmente
        [HttpPost("cerrar/{id}")]
        public async Task<IActionResult> CerrarProceso(int id)
        {
            var proceso = await _context.ProcesoElectorales.FindAsync(id);

            if (proceso == null)
                return NotFound();

            proceso.Activo = false;
            await _context.SaveChangesAsync();

            return Ok("Proceso cerrado correctamente");
        }
        // DELETE: Eliminar un proceso electoral (y sus votos/candidatos en cascada)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProceso(int id)
        {
            var proceso = await _context.ProcesoElectorales.FindAsync(id);
            if (proceso == null) return NotFound();

            _context.ProcesoElectorales.Remove(proceso);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}