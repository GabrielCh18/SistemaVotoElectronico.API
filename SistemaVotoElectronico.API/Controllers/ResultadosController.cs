using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVotoElectronico.API.Data;

namespace SistemaVotoElectronico.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultadosController : ControllerBase
    {
        private readonly VotoContext _context;

        public ResultadosController(VotoContext context)
        {
            _context = context;
        }

        [HttpGet("conteo-detallado")]
        public async Task<IActionResult> GetResultadosDetallados()
        {
            var totalVotos = await _context.Votos.CountAsync();

            // Obtenemos la lista de todos los candidatos con sus conteos
            var todosLosResultados = await _context.Candidatos
                .Select(c => new
                {
                    Nombre = c.Nombre,
                    EsEspecial = c.Nombre.Contains("Voto"), // Detecta si es Blanco o Nulo
                    Votos = _context.Votos.Count(v => v.CandidatoId == c.Id),
                    Porcentaje = totalVotos > 0
                        ? (double)_context.Votos.Count(v => v.CandidatoId == c.Id) / totalVotos * 100
                        : 0
                })
                .ToListAsync();

            // Separamos para mayor claridad en el reporte
            var votosPorCandidatos = todosLosResultados.Where(r => !r.EsEspecial).ToList();
            var votosNulosYBlancos = todosLosResultados.Where(r => r.EsEspecial).ToList();

            return Ok(new
            {
                ResumenGlobal = new
                {
                    TotalEmitidos = totalVotos,
                    VotosValidos = votosPorCandidatos.Sum(v => v.Votos),
                    VotosNulos = votosNulosYBlancos.FirstOrDefault(v => v.Nombre == "Voto Nulo")?.Votos ?? 0,
                    VotosBlancos = votosNulosYBlancos.FirstOrDefault(v => v.Nombre == "Voto en Blanco")?.Votos ?? 0
                },
                DetalleCandidatos = votosPorCandidatos
            });
        }
        [HttpGet("por-provincia/{provinciaId}")]
        public async Task<IActionResult> GetResultadosPorProvincia(int provinciaId)
        {
            var resultados = await _context.Candidatos
                .Select(c => new
                {
                    Candidato = c.Nombre,
                    VotosEnProvincia = _context.Votos
                        .Where(v => v.CandidatoId == c.Id &&
                                    v.Junta.Zona.Parroquia.Canton.ProvinciaId == provinciaId)
                        .Count()
                })
                .ToListAsync();

            return Ok(resultados);
        }
    }
}