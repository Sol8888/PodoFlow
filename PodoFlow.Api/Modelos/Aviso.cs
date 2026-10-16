namespace PodoFlow.Api.Modelos
{
    public class Aviso
    {
        public int AvId { get; set; }

        public string AvVersi { get; set; } = string.Empty;

        public DateOnly AvFechaDesd { get; set; }

        public DateOnly? AvFechaHast { get; set; }

        public string? AvRutaArch { get; set; }

        public string? AvHuella { get; set; }

        public bool AvEstatus { get; set; }
    }
}
