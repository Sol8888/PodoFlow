
namespace PodoFlow.Api.DTOs.BloqueoAgenda;

public class BloqueoAgendaRespuestaDto
{
    public long Id { get; set; }

    public long? UsuarioId { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public string Motivo { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; }

    public bool Estatus { get; set; }
}
