using SistemaVotoElectronico.API.Data;
using SistemaVoto.Modelos;
using System.Linq;

namespace SistemaVotoElectronico.API.Data
{
    public static class DbInitializer
    {
        public static void Initialize(VotoContext context)
        {
            context.Database.EnsureCreated();

            // Evitar duplicados
            if (context.Votantes.Any())
                return;

            // 1️⃣ Ubicación
            var prov = new Provincia { Nombre = "Imbabura" };
            context.Provincias.Add(prov);
            context.SaveChanges();

            var can = new Canton { Nombre = "Ibarra", ProvinciaId = prov.Id };
            context.Cantones.Add(can);
            context.SaveChanges();

            var par = new Parroquia { Nombre = "El Sagrario", CantonId = can.Id };
            context.Parroquias.Add(par);
            context.SaveChanges();

            var zon = new Zona
            {
                Nombre = "Zona UTN",
                Direccion = "Av. 17 de Julio, Ibarra",
                ParroquiaId = par.Id
            };
            context.Zonas.Add(zon);
            context.SaveChanges();

            // 2️⃣ Junta
            var jun = new Junta
            {
                Numero = 1,
                Genero = "Masculino",
                ZonaId = zon.Id
            };
            context.Juntas.Add(jun);
            context.SaveChanges();

            // 3️⃣ Votante (SIN YaVoto)
            var votante = new Votante
            {
                Nombre = "Juan",
                Apellido = "Perez",
                Cedula = "1004567890",
                JuntaId = jun.Id
            };
            context.Votantes.Add(votante);

            // 4️⃣ Candidatos
            context.Candidatos.AddRange(
                new Candidato
                {
                    Nombre = "Candidato A",
                    PartidoPolitico = "Frente UTN",
                    Dignidad = "Presidente",
                    FotoUrl = "a.jpg"
                },
                new Candidato
                {
                    Nombre = "Candidato B",
                    PartidoPolitico = "Alianza Estudiantil",
                    Dignidad = "Presidente",
                    FotoUrl = "b.jpg"
                },
                new Candidato
                {
                    Nombre = "Voto en Blanco",
                    PartidoPolitico = "N/A",
                    Dignidad = "N/A",
                    FotoUrl = "blanco.jpg"
                },
                new Candidato
                {
                    Nombre = "Voto Nulo",
                    PartidoPolitico = "N/A",
                    Dignidad = "N/A",
                    FotoUrl = "nulo.jpg"
                }
            );

            context.SaveChanges();
            System.Console.WriteLine("✅ Base de datos poblada con éxito.");
        }
    }
}