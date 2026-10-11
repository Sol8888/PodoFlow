
namespace PodoFlow.Api.Modelos;

public class RolPermiso
{
    public int RoId { get; set; }

    public int PeId { get; set; }

    public DateTime RpFechaAsig { get; set; }

    public bool RpEstatus { get; set; } = true;
}

