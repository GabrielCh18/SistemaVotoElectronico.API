using Microsoft.AspNetCore.Authorization;
namespace SistemaVotoElectronico.API.Models
{
    public class Voto
    {
        public int Id { get; set; }
        public int EleccionId { get; set; }
        public int CandidatoId { get; set; }
        // Este hash garantiza que el voto no sea modificado (inmutabilidad) [cite: 21]
        public string HashSeguridad { get; set; }

    }
}
