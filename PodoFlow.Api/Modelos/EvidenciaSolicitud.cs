namespace PodoFlow.Api.Modelos
{
    public class EvidenciaSolicitud
    {
        public long EvId { get; set; }

        public long SoId { get; set; }

        public long UsId { get; set; }

        public string EvTipo { get; set; } = string.Empty;

        public string EvRutaArch { get; set; } = string.Empty;

        public string? EvHuella { get; set; }

        public DateTime EvFechaCarg { get; set; }
    }
}
