namespace SistemaVotoElectronico.API.Models
{
    public class Seccion
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int ParroquiaId { get; set; }
    }
}
