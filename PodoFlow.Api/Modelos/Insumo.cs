
namespace PodoFlow.Api.Modelos;

public class Insumo
{
    public int InId { get; set; }

    public string InNombre { get; set; } = string.Empty;

    public string InCateg { get; set; } = string.Empty;

    public string InUnidad { get; set; } = string.Empty;

    public decimal InStockActu { get; set; }

    public decimal InStockMini { get; set; }

    public DateTime InFechaCrea { get; set; }

    public DateTime? InFechaModi { get; set; }

    public bool InEstatus { get; set; } = true;

    public byte[] InVersion { get; set; } = Array.Empty<byte>();
}
