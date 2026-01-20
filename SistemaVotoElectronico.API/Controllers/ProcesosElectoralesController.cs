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

        // GET: Ver todos los procesos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProcesoElectoral>>> GetProcesos()
        {
            return await _context.ProcesoElectorales.ToListAsync();
        }

        // POST: Crear una nueva elección
        [HttpPost]
        public async Task<ActionResult<ProcesoElectoral>> PostProceso(ProcesoElectoral proceso)
        {
            _context.ProcesoElectorales.Add(proceso);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetProcesos", new { id = proceso.Id }, proceso);
        }

        // GET: Solo los activos (para que la app sepa cuál mostrar)
        [HttpGet("activos")]
        public async Task<ActionResult<IEnumerable<ProcesoElectoral>>> GetProcesosActivos()
        {
            var hoy = DateTime.UtcNow;
            // Traemos los que estén marcados como activos
            return await _context.ProcesoElectorales
                .Where(p => p.Activo)
                .ToListAsync();
        }
    }
}