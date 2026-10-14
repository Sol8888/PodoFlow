
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.UsuarioRol;

public class UsuarioRolActualizarDto
{
    [Range(1, int.MaxValue)]
    public int NuevoRolId { get; set; }
}
