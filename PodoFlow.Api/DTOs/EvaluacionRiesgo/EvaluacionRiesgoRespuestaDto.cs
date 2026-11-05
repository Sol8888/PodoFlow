namespace PodoFlow.Api.DTOs.EvaluacionRiesgo
{
    public class EvaluacionRiesgoRespuestaDto
    {
        public long EvaluacionId { get; set; }

        public long RiesgoId { get; set; }

        public DateTime FechaAsignacion { get; set; }

        public string? Observacion { get; set; }
    }
}
