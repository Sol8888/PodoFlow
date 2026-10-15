using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.EvaluacionImpacto
{
    // El estado no se envía: toda evaluación nace en BORRADOR.
    public class EvaluacionImpactoCrearDto
    {
        // Responsable de la evaluación. Obligatorio: us_id es NOT NULL.
        [Required]
        public long UsuarioId { get; set; }

        // Ej.: "1.0", "2025-A". Para cambiar una evaluación aprobada se crea una versión nueva.
        [Required, MaxLength(30)]
        public string Version { get; set; } = string.Empty;

        [Required]
        public DateOnly Fecha { get; set; }

        // Qué tratamientos o procesos cubre la evaluación.
        [Required, MaxLength(2000)]
        public string Alcance { get; set; } = string.Empty;

        [MaxLength(3000)]
        public string? Descripcion { get; set; }

        // Obligatoria para aprobar.
        [MaxLength(3000)]
        public string? Conclusion { get; set; }

        // Riesgo residual global (1 a 25). Obligatorio para aprobar.
        [Range(1, 25)]
        public byte? NivelResidual { get; set; }
    }
}
