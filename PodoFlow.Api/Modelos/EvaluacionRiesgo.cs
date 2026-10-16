namespace PodoFlow.Api.Modelos
{
    public class EvaluacionRiesgo
    {
        public long EvId { get; set; }

        public long RiId { get; set; }

        public DateTime ErFechaAsig { get; set; }

        public string? ErObser { get; set; }
    }
}
