using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface IEncargadoRepositorio
    {
        Task<List<Encargado>> ObtenerTodosAsync(bool incluirInactivos, int pagina, int tamano);

        Task<Encargado?> ObtenerPorIdAsync(int id);

        Task<Encargado?> ObtenerDuplicadoAsync(string nombre, string servicio, int? excluirId);

        Task<Encargado> CrearAsync(Encargado encargado);

        Task GuardarCambiosAsync();
    }
}
