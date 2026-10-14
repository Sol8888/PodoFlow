
namespace PodoFlow.Api.DTOs.Adjunto;

public class AdjuntoRespuestaDto
{
    public long Id { get; set; }

    public long AtencionId { get; set; }

    public long UsuarioId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string RutaArchivo { get; set; } = string.Empty;

    public string? TipoMime { get; set; }

    public string? Huella { get; set; }

    public long? TamanoBytes { get; set; }

    public DateTime FechaCarga { get; set; }

    public bool Estatus { get; set; }
}
