namespace SistemaVoto.Modelos
{
    public class Voto
    {
        public int Id { get; set; }

        public int ProcesoElectoralId { get; set; }
        public ProcesoElectoral? ProcesoElectoral { get; set; }

        public int? CandidatoId { get; set; } 
        public Candidato? Candidato { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public int JuntaId { get; set; }
        public Junta? Junta { get; set; }

        public string? HashIntegridad { get; set; }
    }
}