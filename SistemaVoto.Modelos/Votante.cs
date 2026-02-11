using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVoto.Modelos
{
    public class Votante
    {
        public int Id { get; set; }
        public string Cedula { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public bool EsJefe { get; set; } = false;

        public int JuntaId { get; set; }
        public Junta? Junta { get; set; }
        [NotMapped]
        public bool CertificadoDescargado { get; set; }
        public ICollection<Voto>? Votos { get; set; }

        // PROPIEDADES TEMPORALES (No van a la base de datos)
        [NotMapped]
        public bool YaVoto { get; set; }

        [NotMapped]
        public string? NombreProceso { get; set; }

        [NotMapped]
        public string? Token { get; set; }
    }
}