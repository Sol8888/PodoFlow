using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.EvaluacionImpacto
{
    public class EvaluacionImpactoCambiarEstadoDto
    {
        // BORRADOR, APROBADA, REVISADA o CERRADA.
        [Required, MaxLength(20)]
        public string Estado { get; set; } = string.Empty;
    }
}
