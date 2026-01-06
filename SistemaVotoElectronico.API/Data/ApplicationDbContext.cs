using Microsoft.EntityFrameworkCore;
using SistemaVotoElectronico.API.Models;

namespace SistemaVotoElectronico.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Definición de las tablas del sistema electoral
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Candidato> Candidatos { get; set; }
        public DbSet<Eleccion> Elecciones { get; set; }
        public DbSet<Voto> Votos { get; set; }
    }
}