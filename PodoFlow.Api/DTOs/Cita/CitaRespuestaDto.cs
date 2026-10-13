
namespace PodoFlow.Api.DTOs.Cita;

public class CitaRespuestaDto
{
    public long Id { get; set; }
    public long PacienteId { get; set; }
    public int ServicioId { get; set; }
    public long? UsuarioId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estado { get; set; } = "";
    public string Canal { get; set; } = "";
    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string Version { get; set; } = "";
}
