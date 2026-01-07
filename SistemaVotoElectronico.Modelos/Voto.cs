using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVotoElectronico.Modelos
{
    internal class Voto
    {
        public int Id { get; set; } // Llave Primaria
        public int EleccionId { get; set; }
        public int CandidatoId { get; set; }
        public string HashSeguridad { get; set; } = string.Empty; // Para asegurar que no se altere
    }
}
