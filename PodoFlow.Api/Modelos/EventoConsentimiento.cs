namespace PodoFlow.Api.Modelos
{
    public class EventoConsentimiento
    {
        public long EvId { get; set; }

        public long CoId { get; set; }

        public long? UsId { get; set; }

        public string EvEstad { get; set; } = string.Empty;

        public DateTime EvFechaHora { get; set; }

        public string EvMedio { get; set; } = string.Empty;

        public string? EvRutaEvid { get; set; }

        public string? EvObser { get; set; }
        public Consentimiento? Consentimiento { get; set; }
    }
}
