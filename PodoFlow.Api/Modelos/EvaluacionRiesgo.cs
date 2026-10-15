namespace PodoFlow.Api.Modelos
{
    // Tabla intermedia pri.EV_RI: qué riesgos analiza cada evaluación de impacto.
    // Caso C: no tiene estatus, solo se inserta y se lista.
    public class EvaluacionRiesgo
    {
        public long EvId { get; set; }

        public long RiId { get; set; }

        public DateTime ErFechaAsig { get; set; }

        public string? ErObser { get; set; }
    }
}
