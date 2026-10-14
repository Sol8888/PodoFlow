
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Permiso;

public class PermisoCrearDto
{
    [Required]
    [MaxLength(80)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Modulo { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }
}
