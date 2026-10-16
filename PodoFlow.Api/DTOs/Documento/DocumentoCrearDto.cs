using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Documento
{
    public class DocumentoCrearDto
    {
        [Range(1, long.MaxValue)]
        public long UsuarioId { get; set; }

        [Required, MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Version { get; set; }

        [Required, MaxLength(255)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string RutaArchivo { get; set; } = string.Empty;

        [MaxLength(128)]
        public string? Huella { get; set; }
    }
}
