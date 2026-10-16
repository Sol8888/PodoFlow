using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Consentimiento
{
    public class ConsentimientoCrearDto
    {
        [Required]
        public long PacienteId { get; set; }

        [Required]
        public int FinalidadId { get; set; }

        [Required]
        public int AvisoId { get; set; }

        [Required, MaxLength(15)]
        public string Estado { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Medio { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Observacion { get; set; }

        [MaxLength(1000)]
        public string? RutaEvidencia { get; set; }

        public long? UsuarioId { get; set; }
    }
}

