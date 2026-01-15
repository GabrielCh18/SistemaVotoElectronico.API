using static System.Collections.Specialized.BitVector32;

namespace SistemaVotoElectronico.API.Models
{
    public class Canton
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int ProvinciaId { get; set; }
        public Provincia Provincia { get; set; }
        public List<Parroquia> Parroquias { get; set; }
    }
}
