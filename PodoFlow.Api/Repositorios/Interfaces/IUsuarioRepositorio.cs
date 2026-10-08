
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IUsuarioRepositorio
{
    Task<List<Usuario>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano);

    Task<Usuario?> ObtenerPorIdAsync(long id);
    Task<Usuario?> ObtenerPorCorreoAsync(string correo);
    Task<Usuario> CrearAsync(Usuario usuario);
    Task GuardarCambiosAsync();
}
