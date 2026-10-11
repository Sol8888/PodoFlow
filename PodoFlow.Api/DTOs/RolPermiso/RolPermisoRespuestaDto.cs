
namespace PodoFlow.Api.DTOs.RolPermiso;

public class RolPermisoRespuestaDto
{
    public int RolId { get; set; }

    public int PermisoId { get; set; }

    public DateTime FechaAsignacion { get; set; }

    public bool Estatus { get; set; }
}
