
namespace PodoFlow.Api.Modelos;

public class BloqueoAgenda
{
    public long BlId { get; set; }

    public long? UsId { get; set; }

    public DateTime BlFechaIni { get; set; }

    public DateTime BlFechaFin { get; set; }

    public string BlMotivo { get; set; } = string.Empty;

    public DateTime BlFechaCrea { get; set; }

    public bool BlEstatus { get; set; } = true;
}
