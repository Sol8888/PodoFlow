
namespace PodoFlow.Api.Modelos;

public class Movimiento
{
    public long MoId { get; set; }

    public int InId { get; set; }

    public long? AtId { get; set; }

    public long UsId { get; set; }

    public string MoTipo { get; set; } = string.Empty;

    public decimal MoCant { get; set; }

    public decimal MoStockAnte { get; set; }

    public decimal MoStockNuev { get; set; }

    public DateTime MoFechaHora { get; set; }

    public string MoMotivo { get; set; } = string.Empty;
}
