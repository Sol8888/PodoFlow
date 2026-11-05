using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.EvaluacionRiesgo
{
    public class EvaluacionRiesgoCrearDto
    {
        [Required]
        public long EvaluacionId { get; set; }

        [Required]
        public long RiesgoId { get; set; }

        [MaxLength(1000)]
        public string? Observacion { get; set; }
    }
}
