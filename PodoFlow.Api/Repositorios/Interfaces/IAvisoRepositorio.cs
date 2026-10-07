using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface IAvisoRepositorio
    {
        Task<List<Aviso>> ObtenerTodosAsync();

        Task<Aviso?> ObtenerPorIdAsync(int id);

        Task<Aviso?> ObtenerPorVersionAsync(string version);

        Task<Aviso> CrearAsync(Aviso aviso);

        Task ActualizarAsync(Aviso aviso);

        Task EliminarAsync(Aviso aviso);
    }
}
