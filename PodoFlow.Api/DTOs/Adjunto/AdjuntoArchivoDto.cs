
namespace PodoFlow.Api.DTOs.Adjunto;

public sealed record AdjuntoArchivoDto(
    Stream Contenido,
    string TipoMime,
    string Nombre);
