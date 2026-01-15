namespace SistemaVotoElectronico.API.Models
{
    public class Provincia
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public List<Canton> Cantones { get; set; }
    }
}
