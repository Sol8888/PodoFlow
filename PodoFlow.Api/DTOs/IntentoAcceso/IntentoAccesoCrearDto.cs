
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.IntentoAcceso;

public class IntentoAccesoCrearDto
{
    public long? UsuarioId { get; set; }

    [MaxLength(320)]
    public string? IdentificadorAcceso { get; set; }

    [Required]
    public bool Exitoso { get; set; }

    [MaxLength(300)]
    public string? Motivo { get; set; }
}
