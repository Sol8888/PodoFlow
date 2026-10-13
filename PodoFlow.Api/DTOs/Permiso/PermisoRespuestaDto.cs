
namespace PodoFlow.Api.DTOs.Permiso;

public class PermisoRespuestaDto
{
    public int Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Modulo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public bool Estatus { get; set; }
}
