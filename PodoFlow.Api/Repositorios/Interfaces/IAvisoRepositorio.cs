using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface IAvisoRepositorio
    {
        Task<List<Aviso>> ObtenerTodosAsync(bool incluirInactivos, int pagina, int tamano);

        Task<Aviso?> ObtenerPorIdAsync(int id);

        Task<Aviso?> ObtenerPorVersionAsync(string version);

        Task<Aviso> CrearAsync(Aviso aviso);

        Task GuardarCambiosAsync();
    }
}
