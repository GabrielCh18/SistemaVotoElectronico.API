using Microsoft.EntityFrameworkCore;
using SistemaVotoElectronico.API;
using SistemaVotoElectronico.API.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Voto> Votos { get; set; }
    public DbSet<Provincia> Provincias { get; set; }
    public DbSet<Parroquia> Parroquias { get; set; }
    public DbSet<Seccion> Secciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // La cédula debe ser única para evitar registros duplicados
        modelBuilder.Entity<Usuario>().HasIndex(u => u.Cedula).IsUnique();
    }
}