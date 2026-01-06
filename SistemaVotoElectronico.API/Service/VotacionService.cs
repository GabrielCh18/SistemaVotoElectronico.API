using System.Collections.Generic;
using System.Linq;

public class VotacionService
{
    public Dictionary<string, int> CalcularDHondt(Dictionary<string, int> votosPorLista, int escañosTotales)
    {
        var escañosAsignados = votosPorLista.Keys.ToDictionary(lista => lista, lista => 0); 

        for (int i = 0; i < escañosTotales; i++)
        {
           // Aplicar fórmula: Votos / (EscañosAsignados + 1) 
            var listaGanadora = votosPorLista
                .OrderByDescending(kvp => kvp.Value / (escañosAsignados[kvp.Key] + 1))
                .First().Key;

            escañosAsignados[listaGanadora]++;
        }
        return escañosAsignados;
    }
}