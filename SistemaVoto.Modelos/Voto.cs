using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVoto.Modelos
{
    public class Voto
    {
        [Key]
        public int Id { get; set; }

        // 👇 ¡AQUÍ ESTÁ EL ERROR! Debe llamarse 'FechaVoto'
        public DateTime FechaVoto { get; set; }

        public int IdVotante { get; set; }
        [ForeignKey("IdVotante")]
        public Votante? Votante { get; set; }

        public int? CandidatoId { get; set; }
        [ForeignKey("CandidatoId")]
        public Candidato? Candidato { get; set; }

        public int ProcesoElectoralId { get; set; }
        [ForeignKey("ProcesoElectoralId")]
        public ProcesoElectoral? ProcesoElectoral { get; set; }
    }
}