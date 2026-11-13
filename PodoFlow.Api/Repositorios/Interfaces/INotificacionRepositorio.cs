using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface INotificacionRepositorio
    {
        Task<List<Notificacion>> ObtenerTodosAsync(long? pacienteId, string? estado, int pagina, int tamano);

        Task<List<Notificacion>> ObtenerPorEnviarAsync(DateTime hasta, int tamano);

        Task<Notificacion?> ObtenerPorIdAsync(long id);

        Task<List<Envio>> ObtenerEnviosAsync(long notificacionId);

        Task<DateTime?> ObtenerFechaCitaVigenteAsync(long citaId, long pacienteId);

        Task<List<Cita>> ObtenerCitasPorRecordarAsync(DateTime desde, DateTime hasta);

        Task<bool> ExisteNotificacionDeCitaAsync(long citaId, long opcionId);

        Task<bool> ExisteUsuarioActivoAsync(long usuarioId);

        Task<Notificacion> CrearAsync(Notificacion notificacion);

        Task ActualizarAsync(Notificacion notificacion, byte[] versionLeida);

        Task<Envio> RegistrarEnvioAsync(Notificacion notificacion, Envio envio);
    }
}
