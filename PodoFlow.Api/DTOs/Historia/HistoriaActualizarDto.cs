
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Historia;

public class HistoriaActualizarDto
{
    public long? UsuarioId { get; set; }

    [Required]
    public string Version { get; set; } = string.Empty;
}
