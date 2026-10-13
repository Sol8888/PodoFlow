
namespace PodoFlow.Api.DTOs.UsuarioRol;

public class UsuarioRolRespuestaDto
{
    public long UsuarioId { get; set; }

    public int RolId { get; set; }

    public DateTime FechaAsignacion { get; set; }

    public bool Estatus { get; set; }
}
