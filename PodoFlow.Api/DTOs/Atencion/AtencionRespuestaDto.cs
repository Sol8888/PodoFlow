
namespace PodoFlow.Api.DTOs.Atencion;

public class AtencionRespuestaDto
{
    public long Id { get; set; }
    public long HistoriaId { get; set; }
    public long? CitaId { get; set; }
    public long UsuarioId { get; set; }

    public DateTime Fecha { get; set; }
    public string? MotivoConsulta { get; set; }
    public string? ValoracionClinica { get; set; }
    public string? Diagnostico { get; set; }
    public string? Tratamiento { get; set; }
    public string? Observaciones { get; set; }
    public string Estado { get; set; } = "";
    public DateTime? FechaModificacion { get; set; }
    public string Version { get; set; } = "";
}
