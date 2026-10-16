using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface INotificacionRepositorio
    {
        Task<List<Notificacion>> ObtenerTodosAsync(long? pacienteId, string? estado, int pagina, int tamano);

        // PENDIENTES cuya hora ya llegó, de la más antigua a la más nueva (para el proceso de envío).
        Task<List<Notificacion>> ObtenerPorEnviarAsync(DateTime hasta, int tamano);

        Task<Notificacion?> ObtenerPorIdAsync(long id);

        Task<List<Envio>> ObtenerEnviosAsync(long notificacionId);

        // ci_fecha_ini de la cita si es del paciente y está SOLICITADA, CONFIRMADA o REPROGRAMADA; si no, null.
        Task<DateTime?> ObtenerFechaCitaVigenteAsync(long citaId, long pacienteId);

        Task<bool> ExisteUsuarioActivoAsync(long usuarioId);

        Task<Notificacion> CrearAsync(Notificacion notificacion);

        Task ActualizarAsync(Notificacion notificacion, byte[] versionLeida);

        // Calcula el número de intento, guarda el envío y el nuevo estado de la notificación juntos.
        Task<Envio> RegistrarEnvioAsync(Notificacion notificacion, Envio envio);
    }
}
