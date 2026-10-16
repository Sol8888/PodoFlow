using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Ejecucion
{
    public class RecomendacionCrearDto
    {
        [Required, MaxLength(150)]
        public string Segmento { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Hallazgo { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Sugerencia { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int PacientesEstimados { get; set; }

        [MaxLength(30)]
        public string? NivelPrivacidad { get; set; }
    }
}
