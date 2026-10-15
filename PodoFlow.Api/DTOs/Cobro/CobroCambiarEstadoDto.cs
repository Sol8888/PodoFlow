
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Cobro;

public class CobroCambiarEstadoDto
{
    [Required, MaxLength(20)]
    public string NuevoEstado { get; set; } = "";

    [Required, MaxLength(500)]
    public string Motivo { get; set; } = "";
}
