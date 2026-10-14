
namespace PodoFlow.Api.DTOs.Historia;

public class HistoriaRespuestaDto
{
    public long Id { get; set; }

    public long PacienteId { get; set; }

    public long? UsuarioId { get; set; }

    public DateTime FechaApertura { get; set; }

    public string Estado { get; set; } = string.Empty;

    public DateTime? FechaBloqueo { get; set; }

    public string? MotivoBloqueo { get; set; }

    public string Version { get; set; } = string.Empty;
}
