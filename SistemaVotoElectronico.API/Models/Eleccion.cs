namespace SistemaVotoElectronico.API.Models
{
    public class Eleccion
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public DateTimeOffset FechaInicio { get; set; }
        public DateTimeOffset FechaFin { get; set; }
        public string TipoEleccion { get; set; }
        public bool EstaActiva => DateTimeOffset.UtcNow >= FechaInicio && DateTimeOffset.UtcNow <= FechaFin;
    }
}
