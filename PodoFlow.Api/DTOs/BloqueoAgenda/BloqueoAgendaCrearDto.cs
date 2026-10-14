
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.BloqueoAgenda;

public class BloqueoAgendaCrearDto
{
    public long? UsuarioId { get; set; }

    [Required]
    public DateTime FechaInicio { get; set; }

    [Required]
    public DateTime FechaFin { get; set; }

    [Required]
    [MaxLength(300)]
    public string Motivo { get; set; } = string.Empty;
}
