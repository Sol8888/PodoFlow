
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Evolucion;

public class EvolucionActualizarDto
{
    [Required]
    [MaxLength(3000)]
    public string Descripcion { get; set; } = string.Empty;
}
