using System.ComponentModel.DataAnnotations;

namespace SistemaVoto.Modelos
{
    public class Seccion
    {
        [Key]
        public int Id { get; set; }

        public string Nombre { get; set; } 

        public int ParroquiaId { get; set; }
        public Parroquia? Parroquia { get; set; }
    }
}