using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVotoElectronico.Modelos
{
    internal class Eleccion
    {
        public int Id { get; set; } // Llave Primaria
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string TipoEleccion { get; set; } = "Nominal"; // Nominal o Plancha

    }
}
