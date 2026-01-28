using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVotoElectronico.API.Data;
using SistemaVoto.Modelos;

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

        // GET: api/Resultados/5?provinciaId=1&cantonId=2
        [HttpGet("{procesoId}")]
        public async Task<ActionResult<ResumenGeneral>> GetResultados(
            int procesoId,
            [FromQuery] int? provinciaId,
            [FromQuery] int? cantonId,
            [FromQuery] int? parroquiaId)
        {
            // 1. Verificamos que el proceso exista
            var proceso = await _context.ProcesoElectorales
                                        .Include(p => p.Candidatos)
                                        .FirstOrDefaultAsync(p => p.Id == procesoId);

            if (proceso == null) return NotFound("Proceso electoral no encontrado.");

            // 2. Preparamos la consulta BASE (Votos de este proceso)
            var query = _context.Votos.Where(v => v.ProcesoElectoralId == procesoId);

            // 3. Incluimos toda la cadena geográfica para poder filtrar
            query = query.Include(v => v.Votante)
                         .ThenInclude(vot => vot.Junta)
                         .ThenInclude(j => j.Zona)
                         .ThenInclude(z => z.Parroquia)
                         .ThenInclude(p => p.Canton)
                         .ThenInclude(c => c.Provincia);

            // 4. APLICAMOS LOS FILTROS SI VIENEN
            if (provinciaId.HasValue)
                query = query.Where(v => v.Votante.Junta.Zona.Parroquia.Canton.ProvinciaId == provinciaId);

            if (cantonId.HasValue)
                query = query.Where(v => v.Votante.Junta.Zona.Parroquia.CantonId == cantonId);

            if (parroquiaId.HasValue)
                query = query.Where(v => v.Votante.Junta.Zona.ParroquiaId == parroquiaId);

            // 5. Ejecutamos la consulta ya filtrada
            var votos = await query.ToListAsync();
            int totalVotos = votos.Count;

            var respuesta = new ResumenGeneral
            {
                TotalVotos = totalVotos,
                Estado = proceso.Activo ? "En Curso" : "Finalizado"
            };

            // 6. Contamos votos y armamos resultados
            var idsCandidatosReales = proceso.Candidatos.Select(c => c.Id).ToList();

            foreach (var candidato in proceso.Candidatos)
            {
                int votosCandidato = votos.Count(v => v.CandidatoId == candidato.Id);
                double porcentaje = totalVotos > 0 ? (double)votosCandidato / totalVotos * 100 : 0;

                respuesta.Resultados.Add(new ResultadoVotacion
                {
                    Candidato = candidato.Nombre,
                    Partido = candidato.PartidoPolitico,
                    FotoUrl = candidato.FotoUrl ?? "",
                    CantidadVotos = votosCandidato,
                    Porcentaje = Math.Round(porcentaje, 2)
                });
            }

            // 7. Votos Nulos (Cualquiera que NO sea un candidato válido de la lista)
            int nulos = votos.Count(v => !idsCandidatosReales.Contains(v.CandidatoId));
            if (nulos > 0)
            {
                respuesta.Resultados.Add(new ResultadoVotacion
                {
                    Candidato = "Nulos / Blancos",
                    Partido = "Sistema",
                    FotoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a0/Circle_-_black_simple.svg/800px-Circle_-_black_simple.svg.png",
                    CantidadVotos = nulos,
                    Porcentaje = totalVotos > 0 ? Math.Round((double)nulos / totalVotos * 100, 2) : 0
                });
            }

            // Ordenamos al ganador primero
            respuesta.Resultados = respuesta.Resultados.OrderByDescending(r => r.CantidadVotos).ToList();

            return respuesta;
        }
    }
}