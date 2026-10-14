
using PodoFlow.Api.DTOs.Solicitud;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface ISolicitudServicio
    {
        Task<List<SolicitudRespuestaDto>> ObtenerTodosAsync();

        Task<List<SolicitudRespuestaDto>> ObtenerPorPacienteAsync(long pacienteId);

        Task<SolicitudRespuestaDto?> ObtenerPorIdAsync(long id);

        Task<List<AccionSolicitudRespuestaDto>?> ObtenerAccionesAsync(long id);

        Task<SolicitudRespuestaDto> CrearAsync(SolicitudCrearDto dto);

        Task<bool> ActualizarAsync(long id, SolicitudActualizarDto dto);

        Task<SolicitudRespuestaDto?> CambiarEstadoAsync(long id, SolicitudCambiarEstadoDto dto);
    }
}
