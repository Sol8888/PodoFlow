
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Rol;

public class RolCrearDto
{
    [Required]
    [MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }
}
