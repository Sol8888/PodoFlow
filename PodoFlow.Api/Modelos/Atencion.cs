
namespace PodoFlow.Api.Modelos;

public class Atencion
{
    public long AtId { get; set; }
    public long HiId { get; set; }
    public long? CiId { get; set; }
    public long UsId { get; set; }

    public DateTime AtFecha { get; set; }

    public string? AtMotivCons { get; set; }
    public string? AtValorClin { get; set; }
    public string? AtDiagn { get; set; }
    public string? AtTrata { get; set; }
    public string? AtObser { get; set; }

    public string AtEstad { get; set; } = "ABIERTA";
    public DateTime? AtFechaModi { get; set; }

    public byte[] AtVersion { get; set; } = [];
}
