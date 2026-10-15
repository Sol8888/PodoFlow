namespace PodoFlow.Api.Modelos
{
    // Documentos de cumplimiento LOPDP (RAT, EIPD, matrices, evidencias de control, avisos…).
    public class Documento
    {
        public long DoId { get; set; }

        public long UsId { get; set; }

        public string DoTipo { get; set; } = string.Empty;

        public string? DoVersi { get; set; }

        public string DoNombre { get; set; } = string.Empty;

        public string DoRutaArch { get; set; } = string.Empty;

        public string? DoHuella { get; set; }

        public DateTime DoFechaCarg { get; set; }

        public bool DoEstatus { get; set; }
    }
}
