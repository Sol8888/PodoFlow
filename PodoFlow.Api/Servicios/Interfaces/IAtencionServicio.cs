
using PodoFlow.Api.DTOs.Atencion;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IAtencionServicio
{
    Task<List<AtencionRespuestaDto>> ObtenerTodosAsync(
        long? historiaId, string? estado,
        int pagina, int tamano);

    Task<AtencionRespuestaDto?> ObtenerPorIdAsync(long id);

    Task<AtencionRespuestaDto> CrearAsync(
        AtencionCrearDto dto);

    Task<bool> ActualizarAsync(
        long id, AtencionActualizarDto dto);

    Task<bool> CambiarEstadoAsync(
        long id, AtencionCambiarEstadoDto dto);
}
