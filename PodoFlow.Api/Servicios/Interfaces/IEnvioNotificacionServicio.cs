using PodoFlow.Api.DTOs.Notificacion;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface IEnvioNotificacionServicio
    {
        Task ProcesarAsync(NotificacionRespuestaDto notificacion, CancellationToken cancelacion);
    }
}
