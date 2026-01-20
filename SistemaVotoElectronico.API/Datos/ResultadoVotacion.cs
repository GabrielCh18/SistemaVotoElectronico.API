namespace SistemaVotoElectronico.API.Datos
{
    public class ResultadoVotacion
    {
        public string Candidato { get; set; }
        public string Partido { get; set; }
        public string FotoUrl { get; set; }
        public int CantidadVotos { get; set; }
        public double Porcentaje { get; set; }
    }
    public class ResumenGeneral
    {
        public int TotalVotos { get; set; }
        public string Estado { get; set; } // Porcentaje de actas o estado del proceso
        public List<ResultadoVotacion> Resultados { get; set; } = new();
    }
}
