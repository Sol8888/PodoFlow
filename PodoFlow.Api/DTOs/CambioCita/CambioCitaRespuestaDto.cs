
namespace PodoFlow.Api.DTOs.CambioCita;

public class CambioCitaRespuestaDto
{
    public long Id { get; set; }

    public long CitaId { get; set; }

    public long? UsuarioId { get; set; }

    public string? EstadoAnterior { get; set; }

    public string? EstadoNuevo { get; set; }

    public DateTime? FechaAnterior { get; set; }

    public DateTime? FechaNueva { get; set; }

    public string? Motivo { get; set; }

    public DateTime FechaCambio { get; set; }
}
