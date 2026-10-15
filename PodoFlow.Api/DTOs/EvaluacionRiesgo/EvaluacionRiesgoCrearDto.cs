using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.EvaluacionRiesgo
{
    public class EvaluacionRiesgoCrearDto
    {
        [Required]
        public long EvaluacionId { get; set; }

        [Required]
        public long RiesgoId { get; set; }

        // Ej.: por qué este riesgo entra en la evaluación o cómo se valoró en ella.
        [MaxLength(1000)]
        public string? Observacion { get; set; }
    }
}
