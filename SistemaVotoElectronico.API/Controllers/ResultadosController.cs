using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVotoElectronico.API.Data;
using SistemaVoto.Modelos; // 🔥 IMPORTANTE: Usamos el modelo compartido

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

        // GET: api/Resultados/5
        [HttpGet("{procesoId}")]
        public async Task<ActionResult<ResumenGeneral>> GetResultados(int procesoId)
        {
            // 1. Verificamos que el proceso exista y traemos sus candidatos
            var proceso = await _context.ProcesoElectorales
                                        .Include(p => p.Candidatos)
                                        .FirstOrDefaultAsync(p => p.Id == procesoId);

            if (proceso == null) return NotFound("Proceso electoral no encontrado.");

            // 2. Traemos TODOS los votos de esa elección (aunque esté cerrada)
            var votos = await _context.Votos
                                      .Where(v => v.ProcesoElectoralId == procesoId)
                                      .ToListAsync();

            int totalVotos = votos.Count;

            var respuesta = new ResumenGeneral
            {
                TotalVotos = totalVotos,
                Estado = proceso.Activo ? "En Curso" : "Finalizado"
            };

            // 3. Contamos votos por CADA candidato (incluso los que tienen 0)
            foreach (var candidato in proceso.Candidatos)
            {
                // Contamos cuantos votos tiene este candidato específico
                int votosCandidato = votos.Count(v => v.CandidatoId == candidato.Id);

                // Calculamos porcentaje (protegemos división por cero)
                double porcentaje = totalVotos > 0 ? (double)votosCandidato / totalVotos * 100 : 0;

                respuesta.Resultados.Add(new ResultadoVotacion
                {
                    Candidato = candidato.Nombre, // O candidato.NombreCompleto si así se llama en tu modelo
                    Partido = candidato.PartidoPolitico,
                    FotoUrl = candidato.FotoUrl ?? "",
                    CantidadVotos = votosCandidato,
                    Porcentaje = Math.Round(porcentaje, 2)
                });
            }

            // 4. Agregamos votos Nulos/Blancos (CandidatoId es null)
            int nulos = votos.Count(v => v.CandidatoId == null);
            if (nulos > 0)
            {
                respuesta.Resultados.Add(new ResultadoVotacion
                {
                    Candidato = "Nulos / Blancos",
                    Partido = "Sistema",
                    FotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a0/Circle_-_black_simple.svg/800px-Circle_-_black_simple.svg.png", // Imagen genérica opcional
                    CantidadVotos = nulos,
                    Porcentaje = totalVotos > 0 ? Math.Round((double)nulos / totalVotos * 100, 2) : 0
                });
            }

            // 5. Ordenamos: El ganador (más votos) va primero
            respuesta.Resultados = respuesta.Resultados.OrderByDescending(r => r.CantidadVotos).ToList();

            return respuesta;
        }
    }
}