
namespace PodoFlow.Api.Modelos;

public class Cita
{
    public long CiId { get; set; }

    public long PaId { get; set; }

    public int SeId { get; set; }

    public long? UsId { get; set; }

    public DateTime CiFechaIni { get; set; }

    public DateTime CiFechaFin { get; set; }

    public string CiEstad { get; set; } = "SOLICITADA";

    public string CiCanal { get; set; } = "SISTEMA";

    public string? CiObser { get; set; }

    public DateTime CiFechaCrea { get; set; }

    public DateTime? CiFechaModi { get; set; }

    public byte[] CiVersion { get; set; } = [];
}
