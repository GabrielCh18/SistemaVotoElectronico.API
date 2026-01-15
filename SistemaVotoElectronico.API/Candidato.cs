namespace SistemaVotoElectronico.API.Models
{
    public class Candidato
    {
        public int Id { get; set; }

        public string Nombre { get; set; }

        public string Lista { get; set; }

        // Dignidad a la que postula (ej. Prefecto, Alcalde, Concejal)
        public string Dignidad { get; set; }

        // URL o ruta de la imagen para mostrar en la terminal de voto
        public string FotoUrl { get; set; }

        // Provincia a la que pertenece la candidatura
        public int ProvinciaId { get; set; }
        public Provincia Provincia { get; set; }

        // Parroquia específica (opcional, si es para dignidades locales)
        public int? ParroquiaId { get; set; }
        public Parroquia Parroquia { get; set; }

        // Relación con los votos recibidos (Colección de navegación)
        public List<Voto> Votos { get; set; } = new();
    }
}