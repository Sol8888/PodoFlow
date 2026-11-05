using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Riesgo
{
    public class RiesgoCambiarEstadoDto
    {
        // ABIERTO, MITIGADO, ACEPTADO o CERRADO.
        [Required, MaxLength(20)]
        public string Estado { get; set; } = string.Empty;

        // Para pasar a MITIGADO hacen falta las medidas y el riesgo residual.
        // Si se envían aquí, reemplazan a las que tenga el riesgo.
        [MaxLength(2000)]
        public string? MedidasMitigacion { get; set; }

        [Range(1, 25)]
        public byte? RiesgoResidual { get; set; }
    }
}
