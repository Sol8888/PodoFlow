
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Cita;

public class CitaCambiarEstadoDto
{
    [Required, MaxLength(20)]
    public string NuevoEstado { get; set; } = "";

    [Required, MaxLength(500)]
    public string Motivo { get; set; } = "";

    [Required]
    public string Version { get; set; } = "";
}
