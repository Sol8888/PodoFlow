
namespace PodoFlow.Api.DTOs.EventoAuditoria;

public class EventoAuditoriaRespuestaDto
{
    public long Id { get; set; }

    public long? UsuarioId { get; set; }

    public long? PacienteId { get; set; }

    public DateTime FechaHora { get; set; }

    public string Modulo { get; set; } = string.Empty;

    public string Accion { get; set; } = string.Empty;

    public string? Entidad { get; set; }

    public string? EntidadId { get; set; }

    public string Resultado { get; set; } = string.Empty;

    public string? DireccionIp { get; set; }

    public string? AgenteUsuario { get; set; }

    public Guid? IdCorrelacion { get; set; }

    public string? MetadatosJson { get; set; }
}
