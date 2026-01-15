namespace SistemaVotoElectronico.API
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Cedula { get; set; }
        public string Nombre { get; set; }
        public string Rol { get; set; }
        public string? CodigoAcceso { get; set; }
        public bool YaVoto { get; set; } = false;
        public int SeccionId { get; set; }
    }
}
