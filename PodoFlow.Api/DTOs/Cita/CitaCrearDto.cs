
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Cita;

public class CitaCrearDto
{
    [Range(1, long.MaxValue)]
    public long PacienteId { get; set; }

    [Range(1, int.MaxValue)]
    public int ServicioId { get; set; }

    public long? UsuarioId { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    [Required, MaxLength(30)]
    public string Canal { get; set; } = "SISTEMA";

    [MaxLength(1000)]
    public string? Observaciones { get; set; }
}
