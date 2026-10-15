
using PodoFlow.Api.DTOs.Adjunto;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IAdjuntoServicio
{
    Task<List<AdjuntoRespuestaDto>> ObtenerTodosAsync(
        long? atencionId,
        bool incluirInactivos,
        int pagina,
        int tamano);

    Task<AdjuntoRespuestaDto?> ObtenerPorIdAsync(long id);

    Task<AdjuntoRespuestaDto> CrearAsync(
        AdjuntoCrearDto dto);

    Task<bool> ActualizarAsync(
        long id, AdjuntoActualizarDto dto);

    Task<bool> EliminarAsync(long id);

    Task<bool> ReactivarAsync(long id);
}

