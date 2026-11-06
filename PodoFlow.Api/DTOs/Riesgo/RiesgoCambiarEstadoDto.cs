using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Riesgo
{
    public class RiesgoCambiarEstadoDto
    {
        [Required, MaxLength(20)]
        public string Estado { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? MedidasMitigacion { get; set; }

        [Range(1, 25)]
        public byte? RiesgoResidual { get; set; }
    }
}
