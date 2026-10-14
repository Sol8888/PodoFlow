
namespace PodoFlow.Api.DTOs.PacienteUsuario;

public class PacienteUsuarioRespuestaDto
{
    public long PacienteId { get; set; }
    public long UsuarioId { get; set; }
    public DateTime FechaAsignacion { get; set; }
    public bool Estatus { get; set; }
}
