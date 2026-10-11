
using PodoFlow.Api.DTOs.RolPermiso;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IRolPermisoServicio
{
    Task<List<RolPermisoRespuestaDto>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano);

    Task<RolPermisoRespuestaDto?> ObtenerPorIdAsync(
        int rolId, int permisoId);

    Task<RolPermisoRespuestaDto> CrearAsync(
        RolPermisoCrearDto dto);

    Task<bool> ActualizarAsync(
        int rolId,
        int permisoId,
        RolPermisoActualizarDto dto);

    Task<bool> EliminarAsync(int rolId, int permisoId);

    Task<bool> ReactivarAsync(int rolId, int permisoId);
}
