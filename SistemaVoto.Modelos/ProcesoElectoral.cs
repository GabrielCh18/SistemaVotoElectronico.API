namespace SistemaVoto.Modelos
{
    public class ProcesoElectoral
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Activo { get; set; }
        public ICollection<Candidato>? Candidatos { get; set; }
        public string? Descripcion { get; set; }
    }
}