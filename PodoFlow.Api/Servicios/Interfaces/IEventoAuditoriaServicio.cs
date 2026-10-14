
using PodoFlow.Api.DTOs.EventoAuditoria;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IEventoAuditoriaServicio
{
    // Firma compartida para ambos equipos.
    Task<EventoAuditoriaRespuestaDto> RegistrarAsync(
        long? us_id,
        long? pa_id,
        string ev_modulo,
        string ev_accion,
        string? ev_entid,
        string? ev_entid_id,
        string ev_resul,
        string? ev_direc_ip,
        string? ev_agent_usua,
        string? ev_metad_json);

    Task<EventoAuditoriaRespuestaDto?> ObtenerPorIdAsync(
        long id);

    Task<List<EventoAuditoriaRespuestaDto>> ObtenerTodosAsync(
        long? usuarioId,
        long? pacienteId,
        string? modulo,
        string? resultado,
        DateTime? desde,
        DateTime? hasta,
        int pagina,
        int tamano);
}
