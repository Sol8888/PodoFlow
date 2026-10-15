
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class AdjuntoRepositorio : IAdjuntoRepositorio
{
    private readonly PodoFlowContext _context;

    public AdjuntoRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public async Task<List<Adjunto>> ObtenerTodosAsync(
        long? atencionId,
        bool incluirInactivos,
        int pagina,
        int tamano)
    {
        var consulta = _context.Adjuntos.AsNoTracking();

        if (atencionId.HasValue)
            consulta = consulta.Where(x =>
                x.AtId == atencionId.Value);

        if (!incluirInactivos)
            consulta = consulta.Where(x => x.AdEstatus);

        return await consulta
            .OrderByDescending(x => x.AdFechaCarg)
            .ThenByDescending(x => x.AdId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public Task<Adjunto?> ObtenerPorIdAsync(long id)
    {
        return _context.Adjuntos
            .FirstOrDefaultAsync(x => x.AdId == id);
    }

    public Task<Adjunto?> ObtenerPorRutaAsync(string ruta)
    {
        return _context.Adjuntos
            .FirstOrDefaultAsync(x => x.AdRutaArch == ruta);
    }

    public Task<Atencion?> ObtenerAtencionAsync(long id)
    {
        return _context.Atenciones
            .FirstOrDefaultAsync(x => x.AtId == id);
    }

    public Task<Historia?> ObtenerHistoriaAsync(long id)
    {
        return _context.Historias
            .FirstOrDefaultAsync(x => x.HiId == id);
    }

    public Task<bool> PacienteActivoAsync(long id)
    {
        return _context.Pacientes.AnyAsync(x =>
            x.PaId == id && x.PaEstatus);
    }

    public Task<bool> UsuarioActivoAsync(long id)
    {
        return _context.Usuarios.AnyAsync(x =>
            x.UsId == id && x.UsEstatus);
    }

    public async Task<Adjunto> CrearAsync(Adjunto adjunto)
    {
        _context.Adjuntos.Add(adjunto);

        await _context.SaveChangesAsync();

        return adjunto;
    }

    public Task GuardarCambiosAsync()
    {
        return _context.SaveChangesAsync();
    }
}
