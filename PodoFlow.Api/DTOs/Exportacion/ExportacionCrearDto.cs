using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Exportacion
{
    public class ExportacionCrearDto
    {
        [Required]
        public long SolicitudId { get; set; }

        [Required]
        public long UsuarioId { get; set; }

        [Required, MaxLength(20)]
        public string Formato { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string RutaArchivo { get; set; } = string.Empty;

        [MaxLength(128)]
        public string? Huella { get; set; }

        [MaxLength(2000)]
        public string? Observacion { get; set; }
    }
}
