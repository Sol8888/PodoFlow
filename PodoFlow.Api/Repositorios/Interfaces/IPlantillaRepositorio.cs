using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface IPlantillaRepositorio
    {
        Task<List<Plantilla>> ObtenerTodosAsync(string? canal, string? tipo, bool incluirInactivos, int pagina, int tamano);

        Task<Plantilla?> ObtenerPorIdAsync(int id);

        // Otra plantilla con el mismo nombre, canal y versión (activa o no), excluyendo un id.
        Task<Plantilla?> ObtenerDuplicadoAsync(string nombre, string canal, string version, int? excluirId);

        // Cuántas notificaciones usan la plantilla (todas, o solo las que aún no se envían).
        Task<int> ContarNotificacionesAsync(int plantillaId, bool soloPendientes);

        Task<Plantilla> CrearAsync(Plantilla plantilla);

        Task GuardarCambiosAsync();
    }
}
