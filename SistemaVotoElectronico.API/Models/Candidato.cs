namespace SistemaVotoElectronico.API.Models
{
    public class Candidato
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Lista { get; set; }
        public int EleccionId { get; set; }
        public Eleccion Eleccion { get; set; }
    }
}
