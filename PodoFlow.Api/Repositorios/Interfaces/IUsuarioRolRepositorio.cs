
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IUsuarioRolRepositorio
{
    Task<List<UsuarioRol>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano);

    Task<UsuarioRol?> ObtenerPorIdAsync(long usuarioId, int rolId);

    Task<bool> UsuarioActivoAsync(long usuarioId);

    Task<bool> RolActivoAsync(int rolId);

    Task<UsuarioRol> CrearAsync(UsuarioRol usuarioRol);

    Task GuardarCambiosAsync();

    Task CambiarRolAsync(
        UsuarioRol asignacionActual,
        UsuarioRol? asignacionNueva,
        int nuevoRolId);
}
