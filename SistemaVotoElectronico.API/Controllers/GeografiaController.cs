using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVotoElectronico.API.Data;
using SistemaVoto.Modelos;

namespace SistemaVotoElectronico.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeografiaController : ControllerBase
    {
        private readonly VotoContext _context;

        public GeografiaController(VotoContext context)
        {
            _context = context;
        }

        // --- 1. PROVINCIAS ---
        [HttpGet("provincias")]
        public async Task<ActionResult<List<Provincia>>> GetProvincias()
        {
            return await _context.Provincias.ToListAsync();
        }

        [HttpPost("provincias")]
        public async Task<ActionResult<Provincia>> PostProvincia(Provincia provincia)
        {
            _context.Provincias.Add(provincia);
            await _context.SaveChangesAsync();
            return Ok(provincia);
        }

        // --- 2. CANTONES ---
        [HttpGet("cantones")]
        public async Task<ActionResult<List<Canton>>> GetCantones()
        {
            return await _context.Cantones.Include(c => c.Provincia).ToListAsync();
        }

        [HttpGet("cantones/por-provincia/{provinciaId}")]
        public async Task<ActionResult<List<Canton>>> GetCantonesPorProvincia(int provinciaId)
        {
            return await _context.Cantones.Where(c => c.ProvinciaId == provinciaId).ToListAsync();
        }

        [HttpPost("cantones")]
        public async Task<ActionResult<Canton>> PostCanton(Canton canton)
        {
            // Validar que la provincia exista
            if (!await _context.Provincias.AnyAsync(p => p.Id == canton.ProvinciaId))
                return BadRequest("La Provincia especificada no existe.");

            _context.Cantones.Add(canton);
            await _context.SaveChangesAsync();
            return Ok(canton);
        }

        // --- 3. PARROQUIAS ---
        [HttpGet("parroquias/por-canton/{cantonId}")]
        public async Task<ActionResult<List<Parroquia>>> GetParroquiasPorCanton(int cantonId)
        {
            return await _context.Parroquias.Where(p => p.CantonId == cantonId).ToListAsync();
        }

        [HttpPost("parroquias")]
        public async Task<ActionResult<Parroquia>> PostParroquia(Parroquia parroquia)
        {
            _context.Parroquias.Add(parroquia);
            await _context.SaveChangesAsync();
            return Ok(parroquia);
        }

        // --- 4. ZONAS ---
        [HttpGet("zonas/por-parroquia/{parroquiaId}")]
        public async Task<ActionResult<List<Zona>>> GetZonasPorParroquia(int parroquiaId)
        {
            return await _context.Zonas.Where(z => z.ParroquiaId == parroquiaId).ToListAsync();
        }

        [HttpPost("zonas")]
        public async Task<ActionResult<Zona>> PostZona(Zona zona)
        {
            _context.Zonas.Add(zona);
            await _context.SaveChangesAsync();
            return Ok(zona);
        }

        // --- 5. JUNTAS (MESAS) ---
        [HttpGet("juntas/por-zona/{zonaId}")]
        public async Task<ActionResult<List<Junta>>> GetJuntasPorZona(int zonaId)
        {
            return await _context.Juntas.Where(j => j.ZonaId == zonaId).ToListAsync();
        }

        [HttpPost("juntas")]
        public async Task<ActionResult<Junta>> PostJunta(Junta junta)
        {
            _context.Juntas.Add(junta);
            await _context.SaveChangesAsync();
            return Ok(junta);
        }
    }
}