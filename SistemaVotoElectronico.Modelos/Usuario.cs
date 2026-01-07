using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVotoElectronico.Modelos
{
    internal class Usuario
    {
        public int Id { get; set; } // Llave Primaria
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Rol { get; set; } = "Votante"; // Votante, Administrador, Candidato
        public bool YaVoto { get; set; } = false;
    }
}
