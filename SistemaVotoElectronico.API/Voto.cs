namespace SistemaVotoElectronico.API.Models
{
    public class Voto
    {
        public int Id { get; set; }
        public int CandidatoId { get; set; }

        // Guardamos la JuntaId para poder filtrar los resultados por 
        // Provincia/Cantón sin saber quién votó.
        public int JuntaId { get; set; }
        public DateTime FechaHora { get; set; } = DateTime.Now;
    }
}
