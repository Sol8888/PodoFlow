using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Documento
{
    public class DocumentoCrearDto
    {
        // Quién cargó el documento. Obligatorio: us_id es NOT NULL.
        [Range(1, long.MaxValue)]
        public long UsuarioId { get; set; }

        // RAT, EIPD, MATRIZ_RIESGO, MATRIZ_ENCARGADO, EVIDENCIA_CONTROL, AVISO_PRIVACIDAD u OTRO.
        [Required, MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Version { get; set; }

        [Required, MaxLength(255)]
        public string Nombre { get; set; } = string.Empty;

        // Ruta donde quedó guardado el archivo. La API no sube archivos, solo registra la ruta.
        [Required, MaxLength(1000)]
        public string RutaArchivo { get; set; } = string.Empty;

        [MaxLength(128)]
        public string? Huella { get; set; }
    }
}
