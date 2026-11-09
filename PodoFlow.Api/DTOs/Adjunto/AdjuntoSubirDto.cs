
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Adjunto;

public class AdjuntoSubirDto
{
    [Range(1, long.MaxValue)]
    public long UsuarioId { get; set; }

    [MaxLength(255)]
    public string? Nombre { get; set; }

    [Required]
    public IFormFile? Archivo { get; set; }
}
