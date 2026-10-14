
using PodoFlow.Api.DTOs.Evolucion;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IEvolucionServicio
{
    Task<List<EvolucionRespuestaDto>> ObtenerTodosAsync(
        long? atencionId,
        bool incluirInactivos,
        int pagina,
        int tamano);

    Task<EvolucionRespuestaDto?> ObtenerPorIdAsync(long id);

    Task<EvolucionRespuestaDto> CrearAsync(
        EvolucionCrearDto dto);

    Task<bool> ActualizarAsync(
        long id, EvolucionActualizarDto dto);

    Task<bool> EliminarAsync(long id);

    Task<bool> ReactivarAsync(long id);
}
