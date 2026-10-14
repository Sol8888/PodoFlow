
using PodoFlow.Api.DTOs.Historia;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IHistoriaServicio
{
    Task<List<HistoriaRespuestaDto>> ObtenerTodosAsync(
        string? estado, int pagina, int tamano);

    Task<HistoriaRespuestaDto?> ObtenerPorIdAsync(long id);

    Task<HistoriaRespuestaDto> CrearAsync(
        HistoriaCrearDto dto);

    Task<bool> ActualizarAsync(
        long id, HistoriaActualizarDto dto);

    Task<bool> CambiarEstadoAsync(
        long id, HistoriaCambiarEstadoDto dto);
}
