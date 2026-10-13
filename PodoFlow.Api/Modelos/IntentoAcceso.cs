
namespace PodoFlow.Api.Modelos;

public class IntentoAcceso
{
    public long InId { get; set; }

    public long? UsId { get; set; }

    public string? InIdentAcce { get; set; }

    public DateTime InFechaHora { get; set; }

    public bool InExitoso { get; set; }

    public string? InMotivo { get; set; }

    public string? InDirecIp { get; set; }

    public string? InAgentUsua { get; set; }
}
