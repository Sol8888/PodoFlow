
namespace PodoFlow.Api.Modelos;

public class Evolucion
{
    public long EvId { get; set; }

    public long AtId { get; set; }

    public long UsId { get; set; }

    public DateTime EvFecha { get; set; }

    public string EvDescri { get; set; } = string.Empty;

    public bool EvEstatus { get; set; } = true;
}
