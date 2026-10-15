
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Atencion;

public class AtencionCambiarEstadoDto
{
    [Required, MaxLength(20)]
    public string NuevoEstado { get; set; } = "";

    [Required, MaxLength(500)]
    public string Motivo { get; set; } = "";

    [Required]
    public string Version { get; set; } = "";
}
