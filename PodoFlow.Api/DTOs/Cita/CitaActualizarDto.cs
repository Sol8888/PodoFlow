
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Cita;

public class CitaActualizarDto : CitaCrearDto
{
    [Required]
    public string Version { get; set; } = "";

    [MaxLength(500)]
    public string? MotivoCambio { get; set; }
}
