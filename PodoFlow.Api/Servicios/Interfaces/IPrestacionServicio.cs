
using PodoFlow.Api.DTOs.Prestacion;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IPrestacionServicio
{
    Task<List<PrestacionRespuestaDto>> ObtenerTodosAsync(
        long? pacienteId,
        string? estado,
        int pagina,
        int tamano);

    Task<PrestacionRespuestaDto?> ObtenerPorIdAsync(long id);

    Task<PrestacionRespuestaDto> CrearAsync(
        PrestacionCrearDto dto);

    Task<bool> ActualizarAsync(
        long id,
        PrestacionActualizarDto dto);

    Task<bool> CambiarEstadoAsync(
        long id,
        PrestacionCambiarEstadoDto dto);
}
