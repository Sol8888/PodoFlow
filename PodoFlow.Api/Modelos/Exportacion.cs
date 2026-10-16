namespace PodoFlow.Api.Modelos
{
    public class Exportacion
    {
        public long ExId { get; set; }

        public long SoId { get; set; }

        public long UsId { get; set; }

        public string ExForma { get; set; } = string.Empty;

        public string ExRutaArch { get; set; } = string.Empty;

        public string? ExHuella { get; set; }

        public DateTime ExFechaGene { get; set; }

        public DateTime? ExFechaEntr { get; set; }

        public string ExEstad { get; set; } = string.Empty;
    }
}
