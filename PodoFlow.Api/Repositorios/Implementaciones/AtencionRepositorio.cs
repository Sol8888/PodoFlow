
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class AtencionRepositorio : IAtencionRepositorio
{
    private readonly PodoFlowContext _context;

    public AtencionRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public async Task<List<Atencion>> ObtenerTodosAsync(
        long? historiaId, string? estado,
        int pagina, int tamano)
    {
        var consulta = _context.Atenciones.AsNoTracking();

        if (historiaId.HasValue)
            consulta = consulta.Where(x =>
                x.HiId == historiaId.Value);

        if (!string.IsNullOrWhiteSpace(estado))
            consulta = consulta.Where(x =>
                x.AtEstad == estado);

        return await consulta
            .OrderByDescending(x => x.AtFecha)
            .ThenByDescending(x => x.AtId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public Task<Atencion?> ObtenerPorIdAsync(long id)
    {
        return _context.Atenciones
            .FirstOrDefaultAsync(x => x.AtId == id);
    }

    public Task<Historia?> ObtenerHistoriaAsync(long id)
    {
        return _context.Historias
            .FirstOrDefaultAsync(x => x.HiId == id);
    }

    public Task<Cita?> ObtenerCitaAsync(long id)
    {
        return _context.Citas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CiId == id);
    }

    public Task<bool> PacienteActivoAsync(long id)
    {
        return _context.Pacientes
            .AnyAsync(x => x.PaId == id && x.PaEstatus);
    }

    public Task<bool> UsuarioActivoAsync(long id)
    {
        return _context.Usuarios
            .AnyAsync(x => x.UsId == id && x.UsEstatus);
    }

    public Task<bool> CitaAsociadaAsync(long citaId)
    {
        return _context.Atenciones
            .AnyAsync(x => x.CiId == citaId);
    }

    public async Task<Atencion> CrearAsync(Atencion atencion)
    {
        _context.Atenciones.Add(atencion);
        await _context.SaveChangesAsync();
        return atencion;
    }

    public Task GuardarCambiosAsync()
    {
        return _context.SaveChangesAsync();
    }

    public void EstablecerVersionOriginal(
        Atencion atencion, byte[] version)
    {
        _context.Entry(atencion)
            .Property(x => x.AtVersion)
            .OriginalValue = version;
    }
}
