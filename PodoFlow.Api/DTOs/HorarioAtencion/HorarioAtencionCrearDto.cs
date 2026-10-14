
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.HorarioAtencion;

public class HorarioAtencionCrearDto
{
    public long? UsuarioId { get; set; }

    [Range(1, 7)]
    public byte DiaSemana { get; set; }

    [Required]
    public TimeSpan? HoraInicio { get; set; }

    [Required]
    public TimeSpan? HoraFin { get; set; }

    public DateTime? FechaDesde { get; set; }

    public DateTime? FechaHasta { get; set; }
}
