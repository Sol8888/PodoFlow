using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface IExportacionRepositorio
    {
        Task<List<Exportacion>> ObtenerTodosAsync(int pagina, int tamano);

        Task<List<Exportacion>> ObtenerPorSolicitudAsync(long solicitudId);

        Task<Exportacion?> ObtenerPorIdAsync(long id);

        Task<Exportacion> CrearAsync(Exportacion exportacion, AccionSolicitud accion);

        Task CambiarEstadoAsync(AccionSolicitud accion);
    }
}