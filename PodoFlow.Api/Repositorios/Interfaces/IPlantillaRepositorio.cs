using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface IPlantillaRepositorio
    {
        Task<List<Plantilla>> ObtenerTodosAsync(string? canal, string? tipo, bool incluirInactivos, int pagina, int tamano);

        Task<Plantilla?> ObtenerPorIdAsync(int id);

        Task<Plantilla?> ObtenerDuplicadoAsync(string nombre, string canal, string version, int? excluirId);

        Task<int> ContarNotificacionesAsync(int plantillaId, bool soloPendientes);

        Task<Plantilla> CrearAsync(Plantilla plantilla);

        Task GuardarCambiosAsync();
    }
}
