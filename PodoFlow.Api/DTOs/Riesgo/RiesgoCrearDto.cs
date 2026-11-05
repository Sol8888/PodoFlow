using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Riesgo
{
    // El nivel no se envía: se calcula como probabilidad × impacto.
    // El estado tampoco: todo riesgo nace ABIERTO.
    public class RiesgoCrearDto
    {
        [Required]
        public int TratamientoId { get; set; }

        [Required, MaxLength(2000)]
        public string Descripcion { get; set; } = string.Empty;

        // 1 (muy baja) a 5 (muy alta). Mismo rango que CK_RIESGO_proba.
        [Range(1, 5)]
        public byte Probabilidad { get; set; }

        // 1 (muy bajo) a 5 (muy alto). Mismo rango que CK_RIESGO_impac.
        [Range(1, 5)]
        public byte Impacto { get; set; }

        [MaxLength(2000)]
        public string? MedidasMitigacion { get; set; }

        // Riesgo que queda después de aplicar las medidas (1 a 25).
        [Range(1, 25)]
        public byte? RiesgoResidual { get; set; }

        public DateOnly? FechaRevision { get; set; }
    }
}
