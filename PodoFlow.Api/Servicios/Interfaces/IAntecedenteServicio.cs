
using PodoFlow.Api.DTOs.Antecedente;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IAntecedenteServicio
{
    Task<List<AntecedenteRespuestaDto>> ObtenerTodosAsync(
        long? historiaId,
        bool incluirInactivos,
        int pagina,
        int tamano);

    Task<AntecedenteRespuestaDto?> ObtenerPorIdAsync(long id);

    Task<AntecedenteRespuestaDto> CrearAsync(
        AntecedenteCrearDto dto);

    Task<bool> ActualizarAsync(
        long id,
        AntecedenteActualizarDto dto);

    Task<bool> EliminarAsync(long id);

    Task<bool> ReactivarAsync(long id);
}
