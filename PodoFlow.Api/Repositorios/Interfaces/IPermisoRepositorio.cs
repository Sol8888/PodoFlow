
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IPermisoRepositorio
{
    Task<List<Permiso>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano);

    Task<Permiso?> ObtenerPorIdAsync(int id);

    Task<Permiso?> ObtenerPorCodigoAsync(string codigo);

    Task<Permiso> CrearAsync(Permiso permiso);

    Task GuardarCambiosAsync();
}
