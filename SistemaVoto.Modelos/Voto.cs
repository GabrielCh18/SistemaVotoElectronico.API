using static System.Collections.Specialized.BitVector32;

namespace SistemaVoto.Modelos
{
    public class Voto
    {
        public int Id { get; set; }

        public int CandidatoId { get; set; }
        public Candidato? Candidato { get; set; }
        public DateTime Fecha { get; set; }
        public int JuntaId { get; set; }
        public Junta? Junta { get; set; }

    }
}