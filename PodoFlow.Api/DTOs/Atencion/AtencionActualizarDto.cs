
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Atencion;

public class AtencionActualizarDto
{
    [MaxLength(1000)]
    public string? MotivoConsulta { get; set; }

    [MaxLength(3000)]
    public string? ValoracionClinica { get; set; }

    [MaxLength(2000)]
    public string? Diagnostico { get; set; }

    [MaxLength(3000)]
    public string? Tratamiento { get; set; }

    [MaxLength(3000)]
    public string? Observaciones { get; set; }

    [Required]
    public string Version { get; set; } = "";
}
