
namespace PodoFlow.Api.Modelos;

public class Permiso
{
    public int PeId { get; set; }

    public string PeCodigo { get; set; } = string.Empty;

    public string PeNombre { get; set; } = string.Empty;

    public string PeModulo { get; set; } = string.Empty;

    public string? PeDescri { get; set; }

    public bool PeEstatus { get; set; } = true;
}
