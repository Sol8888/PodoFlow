
namespace PodoFlow.Api.DTOs.HorarioAtencion;

public class HorarioAtencionRespuestaDto
{
    public int Id { get; set; }

    public long? UsuarioId { get; set; }

    public byte DiaSemana { get; set; }

    public TimeSpan HoraInicio { get; set; }

    public TimeSpan HoraFin { get; set; }

    public DateTime? FechaDesde { get; set; }

    public DateTime? FechaHasta { get; set; }

    public bool Estatus { get; set; }
}
