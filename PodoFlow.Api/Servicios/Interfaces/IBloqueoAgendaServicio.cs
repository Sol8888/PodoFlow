
using PodoFlow.Api.DTOs.BloqueoAgenda;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IBloqueoAgendaServicio
{
    Task<List<BloqueoAgendaRespuestaDto>> ObtenerTodosAsync(
        bool incluirInactivos,
        long? usuarioId,
        int pagina,
        int tamano);

    Task<BloqueoAgendaRespuestaDto?> ObtenerPorIdAsync(long id);

    Task<BloqueoAgendaRespuestaDto> CrearAsync(
        BloqueoAgendaCrearDto dto);

    Task<bool> ActualizarAsync(
        long id, BloqueoAgendaActualizarDto dto);

    Task<bool> EliminarAsync(long id);

    Task<bool> ReactivarAsync(long id);
}
