namespace SistemaVoto.Modelos
{
    public class Zona
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public int ParroquiaId { get; set; }
        public Parroquia? Parroquia { get; set; }
        public List<Junta>? Juntas { get; set; }
    }
}
