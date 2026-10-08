
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IRolRepositorio
{
    Task<List<Rol>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano);

    Task<Rol?> ObtenerPorIdAsync(int id);

    Task<Rol?> ObtenerPorCodigoAsync(string codigo);

    Task<Rol> CrearAsync(Rol rol);

    Task GuardarCambiosAsync();
}
