using PodoFlow.Api.DTOs.Consentimiento;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface IConsentimientoServicio
    {
        Task<List<ConsentimientoRespuestaDto>> ObtenerTodosAsync(int pagina, int tamano);

        Task<List<ConsentimientoRespuestaDto>> ObtenerPorPacienteAsync(long pacienteId);

        Task<ConsentimientoRespuestaDto?> ObtenerPorIdAsync(long id);

        Task<ConsentimientoRespuestaDto> CrearAsync(ConsentimientoCrearDto dto);

        Task<bool> ActualizarAsync(long id, ConsentimientoActualizarDto dto);

        Task<ConsentimientoRespuestaDto?> CambiarEstadoAsync(long id, ConsentimientoCambiarEstadoDto dto);
        Task<List<EventoConsentimientoRespuestaDto>?> ObtenerEventosAsync(long id);
    }
}
