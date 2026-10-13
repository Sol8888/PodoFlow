
namespace PodoFlow.Api.Modelos;

public class EventoAuditoria
{
    public long EvId { get; set; }

    public long? UsId { get; set; }

    public long? PaId { get; set; }

    public DateTime EvFechaHora { get; set; }

    public string EvModulo { get; set; } = string.Empty;

    public string EvAccion { get; set; } = string.Empty;

    public string? EvEntid { get; set; }

    public string? EvEntidId { get; set; }

    public string EvResul { get; set; } = string.Empty;

    public string? EvDirecIp { get; set; }

    public string? EvAgentUsua { get; set; }

    public Guid? EvIdCorre { get; set; }

    public string? EvMetadJson { get; set; }
}
