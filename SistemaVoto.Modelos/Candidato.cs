namespace SistemaVoto.Modelos
{
    public class Candidato
    {
        public int Id { get; set; }

        public int ProcesoElectoralId { get; set; }
        public ProcesoElectoral? ProcesoElectoral { get; set; }

        public string Nombre { get; set; }
        public string PartidoPolitico { get; set; }
        public string Dignidad { get; set; }
        public string? FotoUrl { get; set; }
    }
}