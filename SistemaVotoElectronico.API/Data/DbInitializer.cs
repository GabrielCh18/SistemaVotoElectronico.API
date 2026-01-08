using SistemaVotoElectronico.API.Models;
using System;
using System.Linq;

namespace SistemaVotoElectronico.API.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // 1. Asegurar que la base de datos esté creada
            context.Database.EnsureCreated();

            // 2. Si ya hay usuarios, no hacemos nada (evita duplicados)
            if (context.Usuarios.Any()) return;

            // 3. Crear Usuario Administrador de prueba
            var admin = new Usuario
            {
                Nombre = "Admin General",
                Email = "admin@sistema.com",
                Password = "admin123", // Recuerda que en producción debe ser un Hash
                Rol = "Administrador"
            };
            context.Usuarios.Add(admin);

            // 4. Crear un Votante de prueba
            var votante = new Usuario
            {
                Nombre = "Juan Perez",
                Email = "juan@correo.com",
                Password = "voto123",
                Rol = "Votante",
                YaVoto = false
            };
            context.Usuarios.Add(votante);

            // 5. Crear una Elección de prueba
            // C#
            var eleccion = new Eleccion
            {
                Nombre = "Elecciones Seccionales 2026",
                FechaInicio = DateTime.UtcNow.AddDays(-1),
                FechaFin = DateTime.UtcNow.AddDays(7),
                TipoEleccion = "Nominal"
            };
            context.Elecciones.Add(eleccion);
            context.SaveChanges(); // Guardamos para obtener el ID de la elección

            // 6. Crear Candidatos asociados a esa elección
            context.Candidatos.AddRange(
                new Candidato { Nombre = "Candidato A", Lista = "Lista 1 - Progreso", EleccionId = eleccion.Id },
                new Candidato { Nombre = "Candidato B", Lista = "Lista 2 - Unidad", EleccionId = eleccion.Id },
                new Candidato { Nombre = "Candidato C", Lista = "Lista 3 - Futuro", EleccionId = eleccion.Id }
            );

            context.SaveChanges();
        }
    }
}