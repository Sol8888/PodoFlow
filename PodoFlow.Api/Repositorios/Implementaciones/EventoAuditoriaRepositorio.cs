
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class EventoAuditoriaRepositorio
    : IEventoAuditoriaRepositorio
{
    private readonly PodoFlowContext _context;

    public EventoAuditoriaRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public async Task<EventoAuditoria> CrearAsync(
        EventoAuditoria evento)
    {
        _context.EventosAuditoria.Add(evento);
        await _context.SaveChangesAsync();

        return evento;
    }

    public async Task<EventoAuditoria?> ObtenerPorIdAsync(long id)
    {
        return await _context.EventosAuditoria
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EvId == id);
    }

    public async Task<List<EventoAuditoria>> ObtenerTodosAsync(
        long? usuarioId,
        long? pacienteId,
        string? modulo,
        string? resultado,
        DateTime? desde,
        DateTime? hasta,
        int pagina,
        int tamano)
    {
        var consulta = _context.EventosAuditoria.AsNoTracking();

        if (usuarioId.HasValue)
            consulta = consulta.Where(x =>
                x.UsId == usuarioId.Value);

        if (pacienteId.HasValue)
            consulta = consulta.Where(x =>
                x.PaId == pacienteId.Value);

        if (!string.IsNullOrWhiteSpace(modulo))
            consulta = consulta.Where(x =>
                x.EvModulo == modulo);

        if (!string.IsNullOrWhiteSpace(resultado))
            consulta = consulta.Where(x =>
                x.EvResul == resultado);

        if (desde.HasValue)
            consulta = consulta.Where(x =>
                x.EvFechaHora >= desde.Value);

        if (hasta.HasValue)
            consulta = consulta.Where(x =>
                x.EvFechaHora <= hasta.Value);

        return await consulta
            .OrderByDescending(x => x.EvFechaHora)
            .ThenByDescending(x => x.EvId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }
}
