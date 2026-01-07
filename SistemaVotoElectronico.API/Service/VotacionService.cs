using System.Collections.Generic;
using System.Linq;

namespace SistemaVotoElectronico.API.Services
{
    public class VotacionService
    {
        // Cambiamos el nombre a CalcularDHondt para que coincida con el Controller
        public Dictionary<string, int> CalcularDHondt(Dictionary<string, int> votosPorLista, int escañosTotales)
        {
            // Verificamos que haya votos para evitar errores de división por cero
            if (votosPorLista == null || !votosPorLista.Any()) return new Dictionary<string, int>();

            var escañosAsignados = votosPorLista.Keys.ToDictionary(lista => lista, lista => 0);

            for (int i = 0; i < escañosTotales; i++)
            {
                // Aplicamos la fórmula: Votos / (Escaños + 1)
                var listaGanadora = votosPorLista
                    .OrderByDescending(kvp => (double)kvp.Value / (escañosAsignados[kvp.Key] + 1))
                    .First().Key;

                escañosAsignados[listaGanadora]++;
            }
            return escañosAsignados;
        }
    }
}