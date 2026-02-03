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

        [HttpGet("{procesoId}")]
        public async Task<ActionResult<ResumenGeneral>> GetResultados(
            int procesoId,
            [FromQuery] int? provinciaId,
            [FromQuery] int? cantonId,
            [FromQuery] int? parroquiaId)
        {
            // 1. Verificamos proceso
            var proceso = await _context.ProcesoElectorales
                                        .Include(p => p.Candidatos)
                                        .FirstOrDefaultAsync(p => p.Id == procesoId);

            if (proceso == null) return NotFound("Proceso electoral no encontrado.");

            // 2. CONSULTA DE VOTOS (LO QUE SÍ VOTARON)

            var queryVotos = _context.Votos.Where(v => v.ProcesoElectoralId == procesoId);

            // Filtros Geográficos para VOTOS 
            if (provinciaId.HasValue)
                queryVotos = queryVotos.Where(v => v.Votante.Junta.Zona.Parroquia.Canton.ProvinciaId == provinciaId);
            if (cantonId.HasValue)
                queryVotos = queryVotos.Where(v => v.Votante.Junta.Zona.Parroquia.CantonId == cantonId);
            if (parroquiaId.HasValue)
                queryVotos = queryVotos.Where(v => v.Votante.Junta.Zona.ParroquiaId == parroquiaId);

            var listaVotos = await queryVotos.Include(v => v.Votante).ToListAsync();
            int totalVotos = listaVotos.Count;

            // CONSULTA DE EMPADRONADOS (PADRÓN) 
            // Aquí contamos a TODOS los inscritos en esa zona, hayan votado o no.
            var queryVotantes = _context.Votantes.AsQueryable();

            if (provinciaId.HasValue)
                queryVotantes = queryVotantes.Where(v => v.Junta.Zona.Parroquia.Canton.ProvinciaId == provinciaId);
            if (cantonId.HasValue)
                queryVotantes = queryVotantes.Where(v => v.Junta.Zona.Parroquia.CantonId == cantonId);
            if (parroquiaId.HasValue)
                queryVotantes = queryVotantes.Where(v => v.Junta.Zona.ParroquiaId == parroquiaId);

            int totalEmpadronados = await queryVotantes.CountAsync();

            // CÁLCULOS FINALES
            int ausentismo = totalEmpadronados - totalVotos;
            double porcAusentismo = totalEmpadronados > 0
                ? Math.Round(((double)ausentismo / totalEmpadronados) * 100, 2)
                : 0;

            var respuesta = new ResumenGeneral
            {
                TotalVotos = totalVotos,
                TotalEmpadronados = totalEmpadronados, 
                Ausentismo = ausentismo,               
                PorcentajeAusentismo = porcAusentismo, 
                Estado = proceso.Activo ? "En Curso" : "Finalizado"
            };

            // 5. Armamos la lista de candidatos 
            var idsCandidatosReales = proceso.Candidatos.Select(c => c.Id).ToList();

            foreach (var candidato in proceso.Candidatos)
            {
                int votosCandidato = listaVotos.Count(v => v.CandidatoId == candidato.Id);
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

            // 6. Votos Nulos
            int nulos = listaVotos.Count(v => !idsCandidatosReales.Contains(v.CandidatoId));
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

            respuesta.Resultados = respuesta.Resultados.OrderByDescending(r => r.CantidadVotos).ToList();

            return respuesta;
        }
    }
}