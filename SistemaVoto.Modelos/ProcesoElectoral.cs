using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaVoto.Modelos
{
    public class ProcesoElectoral
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = null!; 

        public string? Descripcion { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public bool Activo { get; set; } = true;

        // Relación: Un proceso tiene muchos candidatos
        public List<Candidato> Candidatos { get; set; } = new();

        // Relación: Un proceso tiene muchos votos registrados
        public List<Voto> Votos { get; set; } = new();
    }
}