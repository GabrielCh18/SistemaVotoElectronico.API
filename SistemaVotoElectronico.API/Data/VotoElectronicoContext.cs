using Microsoft.EntityFrameworkCore;
using SistemaVoto.Modelos; // Asegúrate de que esta línea no tenga error

namespace SistemaVotoElectronico.API.Data
{
    public class VotoContext : DbContext
    {
        public VotoContext(DbContextOptions<VotoContext> options) : base(options)
        {
        }

        // --- MÓDULO DE VOTACIÓN ---
        public DbSet<Candidato> Candidatos { get; set; }
        public DbSet<Voto> Votos { get; set; }

        // --- MÓDULO DE UBICACIÓN (Filtros) ---
        public DbSet<Provincia> Provincias { get; set; }
        public DbSet<Parroquia> Parroquias { get; set; }
        public DbSet<Seccion> Secciones { get; set; }

        // Eliminé 'Usuarios' y 'Elecciones' como pediste.
    }
}