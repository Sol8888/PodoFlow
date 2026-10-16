namespace PodoFlow.Api.Modelos
{
    public class Envio
    {
        public long EnId { get; set; }

        public long NoId { get; set; }

        public short EnNumInten { get; set; }

        public DateTime EnFechaHora { get; set; }

        public string EnResul { get; set; } = string.Empty;

        public string? EnIdProve { get; set; }

        public string? EnCodError { get; set; }

        public string? EnDetalError { get; set; }
    }
}
