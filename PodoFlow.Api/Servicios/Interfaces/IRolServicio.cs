
using PodoFlow.Api.DTOs.Rol;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IRolServicio
{
    Task<List<RolRespuestaDto>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano);

    Task<RolRespuestaDto?> ObtenerPorIdAsync(int id);

    Task<RolRespuestaDto> CrearAsync(RolCrearDto dto);

    Task<bool> ActualizarAsync(
        int id, RolActualizarDto dto);

    Task<bool> EliminarAsync(int id);

    Task<bool> ReactivarAsync(int id);
}
