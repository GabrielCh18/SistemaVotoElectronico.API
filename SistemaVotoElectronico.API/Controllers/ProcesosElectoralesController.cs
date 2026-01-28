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
            var ahora = DateTime.Now;

            var proceso = await _context.ProcesoElectorales
                .Include(p => p.Candidatos)
                .FirstOrDefaultAsync(p =>
                    p.Activo &&
                    p.FechaInicio <= ahora &&
                    p.FechaFin >= ahora
                );

            if (proceso == null)
                return NotFound("No existe un proceso electoral activo en este momento.");

            return Ok(proceso);
        }

        // POST: crear proceso electoral
        [HttpPost]
        public async Task<ActionResult> PostProceso(ProcesoElectoral proceso)
        {
            // Validación de fechas
            if (proceso.FechaInicio >= proceso.FechaFin)
            {
                return BadRequest("La fecha de inicio debe ser menor a la fecha de fin.");
            }

            _context.ProcesoElectorales.Add(proceso);
            await _context.SaveChangesAsync();

            return Ok(proceso);
        }

        // PUT: cerrar proceso manualmente
        [HttpPut("cerrar/{id}")]
        public async Task<IActionResult> CerrarProceso(int id)
        {
            var proceso = await _context.ProcesoElectorales.FindAsync(id);

            if (proceso == null)
                return NotFound();

            proceso.Activo = false;
            await _context.SaveChangesAsync();

            return Ok("Proceso cerrado correctamente");
        }
    }
}