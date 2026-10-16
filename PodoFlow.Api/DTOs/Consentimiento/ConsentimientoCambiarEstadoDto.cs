using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Consentimiento
{
    public class ConsentimientoCambiarEstadoDto
    {
        [Required, MaxLength(15)]
        public string Estado { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Medio { get; set; } = string.Empty;
        public int? AvisoId { get; set; }

        [MaxLength(1000)]
        public string? Observacion { get; set; }

        [MaxLength(1000)]
        public string? RutaEvidencia { get; set; }

        public long? UsuarioId { get; set; }

        [Required]
        public byte[] Version { get; set; } = [];
    }
}
