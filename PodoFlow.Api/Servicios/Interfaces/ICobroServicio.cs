
using PodoFlow.Api.DTOs.Cobro;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface ICobroServicio
{
    Task<List<CobroRespuestaDto>> ObtenerTodosAsync(
        long? prestacionId,
        string? estado,
        int pagina,
        int tamano);

    Task<CobroRespuestaDto?> ObtenerPorIdAsync(long id);

    Task<CobroRespuestaDto> CrearAsync(CobroCrearDto dto);

    Task<bool> ActualizarAsync(
        long id,
        CobroActualizarDto dto);

    Task<bool> CambiarEstadoAsync(
        long id,
        CobroCambiarEstadoDto dto);
}
