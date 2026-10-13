
namespace PodoFlow.Api.Modelos;

public class CambioCita
{
    public long CaId { get; set; }

    public long CiId { get; set; }

    public long? UsId { get; set; }

    public string? CaEstadAnte { get; set; }

    public string? CaEstadNuev { get; set; }

    public DateTime? CaFechaAnte { get; set; }

    public DateTime? CaFechaNuev { get; set; }

    public string? CaMotivo { get; set; }

    public DateTime CaFechaCamb { get; set; }
}
