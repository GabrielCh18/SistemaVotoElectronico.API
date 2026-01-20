using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVotoElectronico.API.Data;
using SistemaVotoElectronico.API.Datos;
using SistemaVotoElectronico.API.DTOs;

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
        // El número '5' sería el ID del Proceso Electoral (ej: Elecciones 2026)
        [HttpGet("{procesoId}")]
        public async Task<ActionResult<ResumenGeneral>> GetResultados(int procesoId)
        {
            // 1. Verificamos que el proceso exista
            var proceso = await _context.ProcesoElectorales
                                        .Include(p => p.Candidatos)
                                        .FirstOrDefaultAsync(p => p.Id == procesoId);

            if (proceso == null) return NotFound("Proceso electoral no encontrado.");

            // 2. Traemos todos los votos de ESA elección
            var votos = await _context.Votos
                                      .Where(v => v.ProcesoElectoralId == procesoId)
                                      .ToListAsync();

            int totalVotos = votos.Count;
            var respuesta = new ResumenGeneral
            {
                TotalVotos = totalVotos,
                Estado = proceso.Activo ? "En Curso" : "Finalizado"
            };

            // 3. Magia: Contamos votos por candidato
            foreach (var candidato in proceso.Candidatos)
            {
                int votosCandidato = votos.Count(v => v.CandidatoId == candidato.Id);
                double porcentaje = totalVotos > 0 ? (double)votosCandidato / totalVotos * 100 : 0;

                respuesta.Resultados.Add(new ResultadoVotacion
                {
                    Candidato = candidato.Nombre,
                    Partido = candidato.PartidoPolitico,
                    FotoUrl = candidato.FotoUrl ?? "", // Si es null, ponemos vacío
                    CantidadVotos = votosCandidato,
                    Porcentaje = Math.Round(porcentaje, 2)
                });
            }

            // 4. Agregamos votos nulos/blancos (donde CandidatoId es null)
            int nulos = votos.Count(v => v.CandidatoId == null);
            if (nulos > 0)
            {
                respuesta.Resultados.Add(new ResultadoVotacion
                {
                    Candidato = "Nulos / Blancos",
                    Partido = "Sistema",
                    CantidadVotos = nulos,
                    Porcentaje = totalVotos > 0 ? Math.Round((double)nulos / totalVotos * 100, 2) : 0
                });
            }

            // Ordenamos: El ganador va primero
            respuesta.Resultados = respuesta.Resultados.OrderByDescending(r => r.CantidadVotos).ToList();

            return respuesta;
        }
    }
}