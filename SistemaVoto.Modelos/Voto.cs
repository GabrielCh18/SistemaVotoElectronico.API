using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVoto.Modelos
{
    public class Voto
    {
        [Key]
        public int Id { get; set; }

        public DateTime FechaVoto { get; set; } = DateTime.Now;

        // Votante
        public int IdVotante { get; set; }
        [ForeignKey(nameof(IdVotante))]
        public Votante? Votante { get; set; }

        // Candidato
        public int CandidatoId { get; set; }
        [ForeignKey(nameof(CandidatoId))]
        public Candidato? Candidato { get; set; }

        // Proceso Electoral
        public int ProcesoElectoralId { get; set; }
        [ForeignKey(nameof(ProcesoElectoralId))]
        public ProcesoElectoral? ProcesoElectoral { get; set; }
    }
}