
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.RolPermiso;

public class RolPermisoActualizarDto
{
    [Range(1, int.MaxValue)]
    public int NuevoPermisoId { get; set; }
}
