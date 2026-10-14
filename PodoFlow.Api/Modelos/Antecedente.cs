
namespace PodoFlow.Api.Modelos;

public class Antecedente
{
    public long AnId { get; set; }

    public long HiId { get; set; }

    public long? UsId { get; set; }

    public string AnTipo { get; set; } = string.Empty;

    public string AnDescri { get; set; } = string.Empty;

    public DateTime AnFechaReg { get; set; }

    public bool AnEstatus { get; set; } = true;
}
