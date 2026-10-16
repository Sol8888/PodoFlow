using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Documento
{
    public class DocumentoActualizarDto
    {
        [Required, MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Version { get; set; }

        [Required, MaxLength(255)]
        public string Nombre { get; set; } = string.Empty;
    }
}
