namespace PodoFlow.Api.Modelos
{
    public class AccionSolicitud
    {
        public long AcId { get; set; }

        public long SoId { get; set; }

        public long UsId { get; set; }

        public DateTime AcFechaHora { get; set; }

        public string? AcEstadAnte { get; set; }

        public string? AcEstadNuev { get; set; }

        public string AcAccion { get; set; } = string.Empty;

        public string? AcObser { get; set; }

        public Solicitud? Solicitud { get; set; }
    }
}
