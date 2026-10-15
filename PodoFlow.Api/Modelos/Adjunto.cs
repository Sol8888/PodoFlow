
namespace PodoFlow.Api.Modelos;

public class Adjunto
{
    public long AdId { get; set; }

    public long AtId { get; set; }

    public long UsId { get; set; }

    public string AdNombre { get; set; } = string.Empty;

    public string AdRutaArch { get; set; } = string.Empty;

    public string? AdTipoMime { get; set; }

    public string? AdHuella { get; set; }

    public long? AdTamanByte { get; set; }

    public DateTime AdFechaCarg { get; set; }

    public bool AdEstatus { get; set; } = true;
}
