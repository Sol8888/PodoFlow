
using PodoFlow.Api.DTOs.Permiso;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IPermisoServicio
{
    Task<List<PermisoRespuestaDto>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano);

    Task<PermisoRespuestaDto?> ObtenerPorIdAsync(int id);

    Task<PermisoRespuestaDto> CrearAsync(PermisoCrearDto dto);

    Task<bool> ActualizarAsync(
        int id, PermisoActualizarDto dto);

    Task<bool> EliminarAsync(int id);

    Task<bool> ReactivarAsync(int id);
}
