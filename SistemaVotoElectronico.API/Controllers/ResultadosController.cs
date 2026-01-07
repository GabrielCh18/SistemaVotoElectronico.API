using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVotoElectronico.API.Data;
using SistemaVotoElectronico.API.Services;
using System.Text;

namespace SistemaVotoElectronico.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")] // Solo el admin accede a resultados finales
    public class ResultadosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly VotacionService _votacionService;

        public ResultadosController(ApplicationDbContext context, VotacionService votacionService)
        {
            _context = context;
            _votacionService = votacionService;
        }

        [HttpGet("obtener-ganadores/{eleccionId}/{escaños}")]
        public IActionResult ObtenerGanadores(int eleccionId, int escaños)
        {
            // 1. Agrupar y contar votos por lista desde la BD
            var conteoVotos = _context.Votos
                .Where(v => v.EleccionId == eleccionId)
                .GroupBy(v => v.CandidatoId)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            // 2. Aplicar el método de adjudicación
            var resultado = _votacionService.CalcularDHondt(conteoVotos, escaños);

            return Ok(resultado);
        }

        [HttpGet("descargar-csv/{eleccionId}")]
        public IActionResult DescargarCSV(int eleccionId)
        {
            var votos = _context.Votos.Where(v => v.EleccionId == eleccionId).ToList();

            var csv = new StringBuilder();
            csv.AppendLine("EleccionId,CandidatoId,HashSeguridad");

            foreach (var v in votos)
            {
                csv.AppendLine($"{v.EleccionId},{v.CandidatoId},{v.HashSeguridad}");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "ReporteResultados.csv");
        }
    }
}