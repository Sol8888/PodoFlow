using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.EvaluacionImpacto
{
    public class EvaluacionImpactoActualizarDto
    {
        [Required, MaxLength(30)]
        public string Version { get; set; } = string.Empty;

        [Required]
        public DateOnly Fecha { get; set; }
        [Required, MaxLength(2000)]
        public string Alcance { get; set; } = string.Empty;

        [MaxLength(3000)]
        public string? Descripcion { get; set; }

        [MaxLength(3000)]
        public string? Conclusion { get; set; }

        [Range(1, 25)]
        public byte? NivelResidual { get; set; }
    }
}
