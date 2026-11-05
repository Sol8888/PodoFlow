namespace PodoFlow.Api.Modelos
{
    public class Consentimiento
    {
        public long CoId { get; set; }

        public long PaId { get; set; }

        public int FiId { get; set; }

        public int AvId { get; set; }

        public string CoEstad { get; set; } = string.Empty;

        public string CoMedio { get; set; } = string.Empty;

        public DateTime CoFechaObte { get; set; }

        public DateTime? CoFechaRevo { get; set; }

        public string? CoObser { get; set; }

        public byte[] CoVersion { get; set; } = [];
    }
}
