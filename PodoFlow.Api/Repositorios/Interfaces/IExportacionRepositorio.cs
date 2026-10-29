using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface IExportacionRepositorio
    {
        Task<List<Exportacion>> ObtenerTodosAsync(int pagina, int tamano);

        Task<List<Exportacion>> ObtenerPorSolicitudAsync(long solicitudId);

        Task<Exportacion?> ObtenerPorIdAsync(long id);

        Task<bool> ExisteEvidenciaAsync(long solicitudId);

        Task<List<Dictionary<string, object?>>> ObtenerDatosTitularAsync(string conjunto, long pacienteId);

        Task<Exportacion> CrearAsync(Exportacion exportacion, AccionSolicitud accion);

        Task CambiarEstadoAsync(AccionSolicitud accion);

        Task RegistrarAccionAsync(AccionSolicitud accion);
    }
}