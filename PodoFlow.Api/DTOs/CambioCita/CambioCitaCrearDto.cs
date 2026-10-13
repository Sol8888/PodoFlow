
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.CambioCita;

public class CambioCitaCrearDto
{
    [Range(1, long.MaxValue)]
    public long CitaId { get; set; }

    public long? UsuarioId { get; set; }

    [MaxLength(20)]
    public string? EstadoAnterior { get; set; }

    [MaxLength(20)]
    public string? EstadoNuevo { get; set; }

    public DateTime? FechaAnterior { get; set; }

    public DateTime? FechaNueva { get; set; }

    [MaxLength(500)]
    public string? Motivo { get; set; }
}
