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

            // Si ya hay votantes, no hacemos nada para evitar duplicados
            if (context.Votantes.Any()) return;

            // 1. Ubicación (Provincia -> Canton -> Parroquia -> Zona)
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
                Direccion = "Av. 17 de Julio, Ibarra", // Obligatorio según tu modelo
                ParroquiaId = par.Id
            };
            context.Zonas.Add(zon);
            context.SaveChanges();

            // 2. Junta (Mesa)
            var jun = new Junta { Numero = 1, Genero = "Masculino", ZonaId = zon.Id };
            context.Juntas.Add(jun);
            context.SaveChanges();

            // 3. Votante (Cédula: 1004567890)
            var v1 = new Votante
            {
                Nombre = "Juan",
                Apellido = "Perez", // Obligatorio según tu modelo
                Cedula = "1004567890",
                YaVoto = false,
                JuntaId = jun.Id
            };
            context.Votantes.Add(v1);

            // 4. Candidatos
            context.Candidatos.AddRange(
                new Candidato { Nombre = "Candidato A", PartidoPolitico = "Frente UTN", Dignidad = "Presidente", FotoUrl = "..." },
                new Candidato { Nombre = "Candidato B", PartidoPolitico = "Alianza Estudiantil", Dignidad = "Presidente", FotoUrl = "..." },

    // OPCIONES ESPECIALES
                new Candidato { Nombre = "Voto en Blanco", PartidoPolitico = "N/A", Dignidad = "N/A", FotoUrl = "blanco.jpg" },
                new Candidato { Nombre = "Voto Nulo", PartidoPolitico = "N/A", Dignidad = "N/A", FotoUrl = "nulo.jpg" }
            );

            context.SaveChanges();
            System.Console.WriteLine("--> Base de datos poblada con éxito.");
        }
    }
}