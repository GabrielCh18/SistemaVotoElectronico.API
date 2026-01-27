namespace SistemaVoto.Modelos
{
    public class Votante
    {
        public int Id { get; set; }
        public string Cedula { get; set; } // Único
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public bool YaVoto { get; set; } = false;
        public bool EsJefe { get; set; } = false;

        // Relación con su lugar de votación
        public int JuntaId { get; set; }
        public Junta? Junta { get; set; }
    }
}
