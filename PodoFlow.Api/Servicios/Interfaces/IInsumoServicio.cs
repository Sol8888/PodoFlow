
using PodoFlow.Api.DTOs.Insumo;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IInsumoServicio
{
    Task<List<InsumoRespuestaDto>> ObtenerTodosAsync(
        bool incluirInactivos,
        int pagina,
        int tamano);

    Task<InsumoRespuestaDto?> ObtenerPorIdAsync(int id);

    Task<InsumoRespuestaDto> CrearAsync(
        InsumoCrearDto dto);

    Task<bool> ActualizarAsync(
        int id,
        InsumoActualizarDto dto);

    Task<bool> EliminarAsync(
        int id,
        string version);

    Task<bool> ReactivarAsync(
        int id,
        string version);
}
