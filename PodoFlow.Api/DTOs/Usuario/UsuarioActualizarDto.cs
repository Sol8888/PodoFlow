
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Usuario;

public class UsuarioActualizarDto
{
    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Correo { get; set; } = string.Empty;

    [Required]
    public string Version { get; set; } = string.Empty;
}
