
namespace PodoFlow.Api.Modelos;

public class Prestacion
{
    public long PrId { get; set; }

    public long PaId { get; set; }

    public long? CiId { get; set; }

    public long? AtId { get; set; }

    public int SeId { get; set; }

    public long UsId { get; set; }

    public DateTime PrFechaPres { get; set; }

    public decimal PrPrecioApli { get; set; }

    public decimal PrCant { get; set; } = 1m;

    // Calculado por SQL Server; no se asigna desde C#.
    public decimal PrTotal { get; private set; }

    public string PrEstad { get; set; } = "PENDIENTE";

    public string? PrObser { get; set; }
}
