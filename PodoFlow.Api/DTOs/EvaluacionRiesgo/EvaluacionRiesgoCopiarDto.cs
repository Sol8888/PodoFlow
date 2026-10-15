using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.EvaluacionRiesgo
{
    // Para la versión nueva de una evaluación: copia los riesgos de la versión anterior,
    // menos los que ya no corresponden. Es la forma de "desvincular" que indica el documento,
    // porque EV_RI no tiene columna de baja.
    public class EvaluacionRiesgoCopiarDto
    {
        [Required]
        public long EvaluacionOrigenId { get; set; }

        [Required]
        public long EvaluacionDestinoId { get; set; }

        // Riesgos de la evaluación origen que NO se pasan a la nueva versión.
        public List<long> RiesgosExcluidos { get; set; } = [];
    }
}
