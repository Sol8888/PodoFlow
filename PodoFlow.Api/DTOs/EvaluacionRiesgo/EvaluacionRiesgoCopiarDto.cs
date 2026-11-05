using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.EvaluacionRiesgo
{
    public class EvaluacionRiesgoCopiarDto
    {
        [Required]
        public long EvaluacionOrigenId { get; set; }

        [Required]
        public long EvaluacionDestinoId { get; set; }

        public List<long> RiesgosExcluidos { get; set; } = [];
    }
}
