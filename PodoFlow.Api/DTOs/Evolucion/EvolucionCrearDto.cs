
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Evolucion;

public class EvolucionCrearDto
{
    [Range(1, long.MaxValue)]
    public long AtencionId { get; set; }

    [Range(1, long.MaxValue)]
    public long UsuarioId { get; set; }

    [Required]
    [MaxLength(3000)]
    public string Descripcion { get; set; } = string.Empty;
}
