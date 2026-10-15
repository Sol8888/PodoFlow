namespace PodoFlow.Api.Modelos
{
    public class Incidente
    {
        public long InId { get; set; }

        public long UsId { get; set; }

        public DateTime InFechaDete { get; set; }

        public string InTipo { get; set; } = string.Empty;

        public string InModulo { get; set; } = string.Empty;

        public string InDescri { get; set; } = string.Empty;

        public string? InDatosAfec { get; set; }

        public string? InImpacPreli { get; set; }

        public string InEstad { get; set; } = string.Empty;

        public DateTime? InFechaCier { get; set; }

        public byte[] InVersion { get; set; } = [];
    }
}
