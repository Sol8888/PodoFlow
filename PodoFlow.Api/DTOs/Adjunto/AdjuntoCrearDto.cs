
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Adjunto;

public class AdjuntoCrearDto
{
    [Range(1, long.MaxValue)]
    public long AtencionId { get; set; }

    [Range(1, long.MaxValue)]
    public long UsuarioId { get; set; }

    [Required, MaxLength(255)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string RutaArchivo { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? TipoMime { get; set; }

    [MaxLength(128)]
    public string? Huella { get; set; }

    [Range(0, long.MaxValue)]
    public long? TamanoBytes { get; set; }
}
