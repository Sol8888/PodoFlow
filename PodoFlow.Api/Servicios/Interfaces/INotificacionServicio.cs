using PodoFlow.Api.DTOs.Notificacion;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface INotificacionServicio
    {
        Task<List<NotificacionRespuestaDto>> ObtenerTodosAsync(long? pacienteId, string? estado, int pagina, int tamano);

        Task<List<NotificacionRespuestaDto>> ObtenerPorEnviarAsync(int tamano);

        Task<NotificacionRespuestaDto?> ObtenerPorIdAsync(long id);

        Task<List<EnvioRespuestaDto>?> ObtenerEnviosAsync(long id);

        Task<NotificacionRespuestaDto> CrearAsync(NotificacionCrearDto dto);

        Task<bool> ActualizarAsync(long id, NotificacionActualizarDto dto);

        Task<NotificacionRespuestaDto?> CambiarEstadoAsync(long id, NotificacionCambiarEstadoDto dto);

        Task<EnvioRespuestaDto?> RegistrarEnvioAsync(long id, EnvioCrearDto dto);
    }
}
