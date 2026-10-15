
namespace PodoFlow.Api.Modelos;

public class Cobro
{
    public long CoId { get; set; }

    public long PrId { get; set; }

    public int FoId { get; set; }

    public long UsId { get; set; }

    public DateTime CoFechaPago { get; set; }

    public decimal CoValor { get; set; }

    public string? CoRefe { get; set; }

    public string? CoObser { get; set; }

    public string CoEstad { get; set; } = "REGISTRADO";
}

