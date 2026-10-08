
namespace PodoFlow.Api.DTOs.Usuario;

public class UsuarioRespuestaDto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public bool Estatus { get; set; }
    public string Version { get; set; } = string.Empty;
}
