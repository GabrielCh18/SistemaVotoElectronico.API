namespace SistemaVoto.Modelos
{
    public class TokenVotacion
    {
        public int Id { get; set; }
        public int VotanteId { get; set; }
        public string CodigoUnico { get; set; } // El que genera el Jefe de Junta
        public DateTime FechaExpiracion { get; set; }
        public bool FueUsado { get; set; } = false;
    }
}
