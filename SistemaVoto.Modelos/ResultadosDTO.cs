namespace SistemaVoto.Modelos
{
    // Una sola fila (Un candidato)
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
        public string Estado { get; set; }
        public List<ResultadoVotacion> Resultados { get; set; } = new List<ResultadoVotacion>();

        public int TotalEmpadronados { get; set; } // Padrón total (filtrado)
        public int Ausentismo { get; set; }        // Cuántos faltaron
        public double PorcentajeAusentismo { get; set; }
    }
}