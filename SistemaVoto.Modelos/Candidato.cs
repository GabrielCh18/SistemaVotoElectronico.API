namespace SistemaVoto.Modelos
{
    public class Candidato
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string PartidoPolitico { get; set; }
        public string Dignidad { get; set; } // Ejemplo: "Presidente"
        public string FotoUrl { get; set; }
    }
}