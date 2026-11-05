namespace PodoFlow.Api.Modelos
{
    // EIPD: Evaluación de Impacto relativa a la Protección de Datos.
    public class EvaluacionImpacto
    {
        public long EvId { get; set; }

        public long UsId { get; set; }

        public string EvVersi { get; set; } = string.Empty;

        public DateOnly EvFecha { get; set; }

        public string EvAlcan { get; set; } = string.Empty;

        public string? EvDescri { get; set; }

        public string? EvConclu { get; set; }

        public byte? EvNivelResid { get; set; }

        public string EvEstad { get; set; } = string.Empty;

        public DateTime? EvFechaModi { get; set; }
    }
}
