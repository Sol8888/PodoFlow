using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Consentimiento
{
    public class ConsentimientoActualizarDto
    {
        [Required, MaxLength(50)]
        public string Medio { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Observacion { get; set; }

        [Required]
        public byte[] Version { get; set; } = [];
    }
}
