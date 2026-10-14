
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IRolPermisoRepositorio
{
    Task<List<RolPermiso>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano);

    Task<RolPermiso?> ObtenerPorIdAsync(int rolId, int permisoId);

    Task<bool> RolActivoAsync(int rolId);

    Task<bool> PermisoActivoAsync(int permisoId);

    Task<RolPermiso> CrearAsync(RolPermiso relacion);

    Task GuardarCambiosAsync();

    Task CambiarPermisoAsync(
        RolPermiso anterior,
        RolPermiso? nueva,
        int nuevoPermisoId);
}
