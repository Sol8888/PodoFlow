
namespace PodoFlow.Api.Modelos;

public class UsuarioRol
{
    public long UsId { get; set; }

    public int RoId { get; set; }

    public DateTime UrFechaAsig { get; set; }

    public bool UrEstatus { get; set; } = true;
}
