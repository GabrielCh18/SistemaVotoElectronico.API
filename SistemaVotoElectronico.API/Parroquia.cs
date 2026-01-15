using static System.Collections.Specialized.BitVector32;

namespace SistemaVotoElectronico.API.Models
{
    public class Parroquia
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int ProvinciaId { get; set; }
        public List<Seccion> Secciones { get; set; } = new();
    }
}
