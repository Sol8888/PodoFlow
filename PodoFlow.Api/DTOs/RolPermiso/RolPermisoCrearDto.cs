
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.RolPermiso;

public class RolPermisoCrearDto
{
    [Range(1, int.MaxValue)]
    public int RolId { get; set; }

    [Range(1, int.MaxValue)]
    public int PermisoId { get; set; }
}
