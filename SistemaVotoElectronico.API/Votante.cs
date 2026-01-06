namespace SistemaVotoElectronico.API
{
    public class Votante
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Votante(int id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        public override string ToString()
        {
            return $"Votante(Id: {Id}, Name: {Name}, Description: {Description})";
        }
    }
}
