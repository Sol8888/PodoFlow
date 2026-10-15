
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Prestacion;

public class PrestacionCambiarEstadoDto
{
    [Required, MaxLength(20)]
    public string NuevoEstado { get; set; } = "";

    [Required, MaxLength(1000)]
    public string Motivo { get; set; } = "";
}
