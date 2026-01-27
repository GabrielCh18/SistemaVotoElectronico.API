namespace SistemaVoto.Modelos
{
    public class Parroquia
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int CantonId { get; set; }
        public Canton? Canton { get; set; }
        public List<Zona>? Zonas { get; set; }
    }
}
