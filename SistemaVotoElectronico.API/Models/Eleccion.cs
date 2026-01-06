namespace SistemaVotoElectronico.API.Models
{
    public class Eleccion
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string TipoEleccion { get; set; } // "Nominal" o "Plancha" [cite: 18]
        public bool EstaActiva => DateTime.Now >= FechaInicio && DateTime.Now <= FechaFin;
    }
}
