
namespace PodoFlow.Api.DTOs.IntentoAcceso;

public class IntentoAccesoRespuestaDto
{
    public long Id { get; set; }

    public long? UsuarioId { get; set; }

    public string? IdentificadorAcceso { get; set; }

    public DateTime FechaHora { get; set; }

    public bool Exitoso { get; set; }

    public string? Motivo { get; set; }

    public string? DireccionIp { get; set; }

    public string? AgenteUsuario { get; set; }
}
