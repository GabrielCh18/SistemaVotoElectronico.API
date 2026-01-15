namespace SistemaVotoElectronico.API.Models
{
    public class Voto
    {
        public int Id { get; set; }
        public int CandidatoId { get; set; }
        public int SeccionId { get; set; } // Permite reconstruir reportes geográficos
        public string HashSeguridad { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
