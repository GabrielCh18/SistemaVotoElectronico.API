using static System.Collections.Specialized.BitVector32;

namespace SistemaVoto.Modelos
{
    public class Voto
    {
        public int Id { get; set; }

        public int CandidatoId { get; set; }
        public Candidato? Candidato { get; set; }

        public int SeccionId { get; set; }
        public Seccion? Seccion { get; set; }

    }
}