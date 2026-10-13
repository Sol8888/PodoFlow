
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.UsuarioRol;

public class UsuarioRolCrearDto
{
    [Range(1, long.MaxValue)]
    public long UsuarioId { get; set; }

    [Range(1, int.MaxValue)]
    public int RolId { get; set; }
}
