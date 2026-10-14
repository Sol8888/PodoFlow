
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class HistoriaRepositorio : IHistoriaRepositorio
{
    private readonly PodoFlowContext _context;

    public HistoriaRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public async Task<List<Historia>> ObtenerTodosAsync(
        string? estado, int pagina, int tamano)
    {
        var consulta = _context.Historias.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(estado))
            consulta = consulta.Where(x => x.HiEstad == estado);

        return await consulta
            .OrderByDescending(x => x.HiFechaAper)
            .ThenByDescending(x => x.HiId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public Task<Historia?> ObtenerPorIdAsync(long id)
    {
        return _context.Historias
            .FirstOrDefaultAsync(x => x.HiId == id);
    }

    public Task<Historia?> ObtenerPorPacienteAsync(
        long pacienteId)
    {
        return _context.Historias
            .FirstOrDefaultAsync(x => x.PaId == pacienteId);
    }

    public Task<bool> PacienteActivoAsync(long pacienteId)
    {
        return _context.Pacientes.AnyAsync(x =>
            x.PaId == pacienteId && x.PaEstatus);
    }

    public Task<bool> UsuarioActivoAsync(long usuarioId)
    {
        return _context.Usuarios.AnyAsync(x =>
            x.UsId == usuarioId && x.UsEstatus);
    }

    public async Task<Historia> CrearAsync(Historia historia)
    {
        _context.Historias.Add(historia);
        await _context.SaveChangesAsync();
        return historia;
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void EstablecerVersionOriginal(
        Historia historia, byte[] version)
    {
        _context.Entry(historia)
            .Property(x => x.HiVersion)
            .OriginalValue = version;
    }
}
