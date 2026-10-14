
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class InsumoRepositorio : IInsumoRepositorio
{
    private readonly PodoFlowContext _context;

    public InsumoRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public Task<List<Insumo>> ObtenerTodosAsync(
        bool incluirInactivos,
        int pagina,
        int tamano)
    {
        IQueryable<Insumo> consulta =
            _context.Insumos.AsNoTracking();

        if (!incluirInactivos)
            consulta = consulta.Where(x => x.InEstatus);

        return consulta
            .OrderBy(x => x.InNombre)
            .ThenBy(x => x.InId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public Task<Insumo?> ObtenerPorIdAsync(int id)
    {
        return _context.Insumos
            .FirstOrDefaultAsync(x => x.InId == id);
    }

    public Task<List<Insumo>> BuscarIdenticosAsync(
        string nombre,
        string categoria,
        string unidad)
    {
        return _context.Insumos
            .Where(x =>
                x.InNombre == nombre &&
                x.InCateg == categoria &&
                x.InUnidad == unidad)
            .OrderByDescending(x => x.InEstatus)
            .ThenBy(x => x.InId)
            .ToListAsync();
    }

    public Task<int> ContarAsync(bool incluirInactivos)
    {
        IQueryable<Insumo> consulta = _context.Insumos;

        if (!incluirInactivos)
            consulta = consulta.Where(x => x.InEstatus);

        return consulta.CountAsync();
    }

    public async Task AgregarAsync(Insumo insumo)
    {
        await _context.Insumos.AddAsync(insumo);
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void EstablecerVersionOriginal(
        Insumo insumo,
        byte[] version)
    {
        _context.Entry(insumo)
            .Property(x => x.InVersion)
            .OriginalValue = version;
    }
}
