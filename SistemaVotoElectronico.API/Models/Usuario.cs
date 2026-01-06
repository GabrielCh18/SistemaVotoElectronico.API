namespace SistemaVotoElectronico.API.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } // Siempre encriptada [cite: 21]
        public string Rol { get; set; } // "Administrador", "Votante", "Candidato" [cite: 15]
        public bool YaVoto { get; set; } = false; // Garantiza un solo voto [cite: 19]
    }
}
