using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Aviso
{
    public class AvisoCrearDto
    {
        [Required, MaxLength(30)]
        public string Version { get; set; } = string.Empty;

        [Required]
        public DateOnly FechaDesde { get; set; }

        public DateOnly? FechaHasta { get; set; }

        [MaxLength(1000)]
        public string? RutaArchivo { get; set; }

        [MaxLength(128)]
        public string? Huella { get; set; }
    }
}
