using Microsoft.EntityFrameworkCore;
using SistemaVoto.Modelos;

namespace SistemaVotoElectronico.API.Data
{
    public class VotoContext : DbContext
    {
        public VotoContext(DbContextOptions<VotoContext> options) : base(options) { }

        //  MÓDULO DE VOTACIÓN 
        public DbSet<Candidato> Candidatos { get; set; }
        public DbSet<Voto> Votos { get; set; }
        public DbSet<Votante> Votantes { get; set; } //
        public DbSet<TokenVotacion> Tokens { get; set; } //

        //  MÓDULO DE UBICACIÓN (Para filtros y mesas) 
        public DbSet<Provincia> Provincias { get; set; }
        public DbSet<Canton> Cantones { get; set; }
        public DbSet<Parroquia> Parroquias { get; set; }
        public DbSet<Zona> Zonas { get; set; }
        public DbSet<Junta> Juntas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // La cédula debe ser única en el padrón
            modelBuilder.Entity<Votante>().HasIndex(v => v.Cedula).IsUnique();

            // El código de votación también debe ser único
            modelBuilder.Entity<TokenVotacion>().HasIndex(t => t.CodigoUnico).IsUnique();

            // RELACIÓN PROCESO ELECTORAL → CANDIDATOS
            modelBuilder.Entity<Candidato>()
                .HasOne(c => c.ProcesoElectoral)
                .WithMany(p => p.Candidatos)
                .HasForeignKey(c => c.ProcesoElectoralId)
                .OnDelete(DeleteBehavior.Cascade);
        }
        public DbSet<ProcesoElectoral> ProcesoElectorales { get; set; }
    }
}