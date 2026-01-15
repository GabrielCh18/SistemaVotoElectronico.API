using Microsoft.EntityFrameworkCore;
using SistemaVotoElectronico.API.Models;
using SistemaVotoElectronico.API;

namespace SistemaVotoElectronico.API.Data
{
    public class VotoElectronicoContext : DbContext
    {
        public VotoElectronicoContext(DbContextOptions<VotoElectronicoContext> options)
            : base(options)
        {
        }

        // Tablas Geográficas
        public DbSet<Provincia> Provincias { get; set; }
        public DbSet<Canton> Cantones { get; set; }
        public DbSet<Parroquia> Parroquias { get; set; }
        public DbSet<Zona> Zonas { get; set; }
        public DbSet<Junta> Juntas { get; set; }

        // Tablas Principales
        public DbSet<Votante> Votantes { get; set; }
        public DbSet<Candidato> Candidatos { get; set; }
        public DbSet<TokenVotacion> Tokens { get; set; }
        public DbSet<Voto> Votos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Garantiza que un número de cédula sea único en la base de datos
            modelBuilder.Entity<Votante>()
                .HasIndex(v => v.Cedula)
                .IsUnique();

            // Garantiza que el código entregado por el Jefe de Junta sea único
            modelBuilder.Entity<TokenVotacion>()
                .HasIndex(t => t.CodigoUnico)
                .IsUnique();
        }
    }
}