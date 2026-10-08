
namespace PodoFlow.Api.DTOs.Rol;

public class RolRespuestaDto
{
    public int Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public bool Estatus { get; set; }
}
