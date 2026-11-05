using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Exportacion
{
    public class ExportacionCrearDto
    {
        [Range(1, long.MaxValue)]
        public long SolicitudId { get; set; }

        [Range(1, long.MaxValue)]
        public long UsuarioId { get; set; }

        [Required, MaxLength(20)]
        public string Formato { get; set; } = string.Empty;

        public List<string>? Secciones { get; set; }

        [MaxLength(2000)]
        public string? Observacion { get; set; }
    }
}
