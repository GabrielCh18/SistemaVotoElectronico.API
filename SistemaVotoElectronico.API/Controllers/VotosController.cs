using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVotoElectronico.API.Data;
using SistemaVoto.Modelos;

namespace SistemaVotoElectronico.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VotosController : ControllerBase
    {
        private readonly VotoContext _context;

        public VotosController(VotoContext context)
        {
            _context = context;
        }

        [HttpPost("registrar-voto")]
public async Task<IActionResult> RegistrarVoto(string cedula, string codigo, int candidatoId)
{
    // 1. Validaciones de Token y Votante (Igual que antes)
    var token = await _context.Tokens.FirstOrDefaultAsync(t => t.CodigoUnico == codigo && !t.FueUsado);
    if (token == null) return BadRequest("Código inválido.");

    var votante = await _context.Votantes.FirstOrDefaultAsync(v => v.Cedula == cedula);
    if (votante == null || votante.YaVoto) return BadRequest("El ciudadano no puede votar.");

    // 2. Creamos el voto capturando la Junta del votante
    var nuevoVoto = new Voto
    {
        CandidatoId = candidatoId,
        Fecha = DateTime.UtcNow,
        JuntaId = votante.JuntaId // <-- AQUÍ capturamos la ubicación automáticamente
    };

    // 3. Marcamos como usado y guardamos
    votante.YaVoto = true;
    token.FueUsado = true;

    _context.Votos.Add(nuevoVoto);
    await _context.SaveChangesAsync();

    return Ok("Voto registrado con éxito.");
}
    }
}