
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class EvolucionRepositorio : IEvolucionRepositorio
{
    private readonly PodoFlowContext _context;

    public EvolucionRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public async Task<List<Evolucion>> ObtenerTodosAsync(
        long? atencionId,
        bool incluirInactivos,
        int pagina,
        int tamano)
    {
        var consulta = _context.Evoluciones.AsNoTracking();

        if (atencionId.HasValue)
        {
            consulta = consulta.Where(x =>
                x.AtId == atencionId.Value);
        }

        if (!incluirInactivos)
        {
            consulta = consulta.Where(x => x.EvEstatus);
        }

        return await consulta
            .OrderByDescending(x => x.EvFecha)
            .ThenByDescending(x => x.EvId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public Task<Evolucion?> ObtenerPorIdAsync(long id)
    {
        return _context.Evoluciones
            .FirstOrDefaultAsync(x => x.EvId == id);
    }

    public Task<Evolucion?> BuscarIgualAsync(
        long atencionId,
        long usuarioId,
        string descripcion)
    {
        return _context.Evoluciones.FirstOrDefaultAsync(x =>
            x.AtId == atencionId &&
            x.UsId == usuarioId &&
            x.EvDescri == descripcion);
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

    public async Task<Evolucion> CrearAsync(Evolucion evolucion)
    {
        _context.Evoluciones.Add(evolucion);
        await _context.SaveChangesAsync();

        return evolucion;
    }

    public Task GuardarCambiosAsync()
    {
        return _context.SaveChangesAsync();
    }
}
