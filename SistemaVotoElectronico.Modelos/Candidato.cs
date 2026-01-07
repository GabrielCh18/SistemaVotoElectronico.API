using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVotoElectronico.Modelos
{
    internal class Candidato
    {
        public int Id { get; set; } // Llave Primaria
        public string Nombre { get; set; } = string.Empty;
        public string Lista { get; set; } = string.Empty;

        // Llave Foránea: Un candidato pertenece a una elección
        public int EleccionId { get; set; }
        public Eleccion? Eleccion { get; set; } // Propiedad de navegación
    }
}
