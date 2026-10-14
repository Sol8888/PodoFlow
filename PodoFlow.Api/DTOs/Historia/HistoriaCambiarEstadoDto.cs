
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Historia;

public class HistoriaCambiarEstadoDto
{
    [Required]
    [MaxLength(20)]
    public string NuevoEstado { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Motivo { get; set; }

    [Required]
    public string Version { get; set; } = string.Empty;
}
