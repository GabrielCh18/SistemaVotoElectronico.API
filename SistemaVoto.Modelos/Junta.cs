
namespace SistemaVoto.Modelos
{
    public class Junta
    {
        public int Id { get; set; }
        public int Numero { get; set; } // Ejemplo: Mesa 1
        public string Genero { get; set; } // Masculino / Femenino
        public int ZonaId { get; set; }
        public Zona? Zona { get; set; }
    }
}
