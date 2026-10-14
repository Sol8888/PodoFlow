
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IEventoAuditoriaRepositorio
{
    Task<EventoAuditoria> CrearAsync(
        EventoAuditoria evento);

    Task<EventoAuditoria?> ObtenerPorIdAsync(long id);

    Task<List<EventoAuditoria>> ObtenerTodosAsync(
        long? usuarioId,
        long? pacienteId,
        string? modulo,
        string? resultado,
        DateTime? desde,
        DateTime? hasta,
        int pagina,
        int tamano);
}
