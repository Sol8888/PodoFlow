
namespace PodoFlow.Api.DTOs.Evolucion;

public class EvolucionRespuestaDto
{
    public long Id { get; set; }

    public long AtencionId { get; set; }

    public long UsuarioId { get; set; }

    public DateTime Fecha { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public bool Estatus { get; set; }
}
