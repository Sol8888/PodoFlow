using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface IRiesgoRepositorio
    {
        Task<List<Riesgo>> ObtenerTodosAsync(int? tratamientoId, string? estado, int pagina, int tamano);

        Task<Riesgo?> ObtenerPorIdAsync(long id);

        Task<Riesgo> CrearAsync(Riesgo riesgo);

        Task GuardarCambiosAsync();
    }
}
