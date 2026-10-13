
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class BloqueoAgendaRepositorio
    : IBloqueoAgendaRepositorio
{
    private readonly PodoFlowContext _context;

    public BloqueoAgendaRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public async Task<List<BloqueoAgenda>> ObtenerTodosAsync(
        bool incluirInactivos,
        long? usuarioId,
        int pagina,
        int tamano)
    {
        var consulta = _context.BloqueosAgenda.AsNoTracking();

        if (!incluirInactivos)
            consulta = consulta.Where(x => x.BlEstatus);

        if (usuarioId.HasValue)
            consulta = consulta.Where(x =>
                x.UsId == usuarioId.Value || x.UsId == null);

        return await consulta
            .OrderByDescending(x => x.BlFechaIni)
            .ThenByDescending(x => x.BlId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public Task<BloqueoAgenda?> ObtenerPorIdAsync(long id)
    {
        return _context.BloqueosAgenda
            .FirstOrDefaultAsync(x => x.BlId == id);
    }

    public Task<bool> UsuarioActivoAsync(long usuarioId)
    {
        return _context.Usuarios.AnyAsync(x =>
            x.UsId == usuarioId && x.UsEstatus);
    }

    public Task<BloqueoAgenda?> BuscarIgualAsync(
        long? usuarioId,
        DateTime inicio,
        DateTime fin,
        string motivo)
    {
        return _context.BloqueosAgenda.FirstOrDefaultAsync(x =>
            x.UsId == usuarioId &&
            x.BlFechaIni == inicio &&
            x.BlFechaFin == fin &&
            x.BlMotivo == motivo);
    }

    public Task<bool> ExisteSolapamientoAsync(
        long? usuarioId,
        DateTime inicio,
        DateTime fin,
        long? excluirId = null)
    {
        return _context.BloqueosAgenda.AsNoTracking()
            .AnyAsync(x =>
                x.BlEstatus &&
                (!excluirId.HasValue ||
                    x.BlId != excluirId.Value) &&

                // Un bloqueo general afecta a todos.
                (usuarioId == null ||
                 x.UsId == null ||
                 x.UsId == usuarioId) &&

                // Intervalos [inicio, fin).
                x.BlFechaIni < fin &&
                inicio < x.BlFechaFin);
    }

    public async Task CrearAsync(BloqueoAgenda bloqueo)
    {
        _context.BloqueosAgenda.Add(bloqueo);
        await _context.SaveChangesAsync();
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }
}
