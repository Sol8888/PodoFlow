namespace PodoFlow.Api.Modelos
{
    public class DatoAnalitico
    {
        public long DaId { get; set; }

        public long EjId { get; set; }

        public Guid SeId { get; set; }

        public int DaFrecAten { get; set; }

        public string? DaCategServ { get; set; }

        public DateOnly? DaFechaUlti { get; set; }

        public int? DaDiasInact { get; set; }
    }
}
