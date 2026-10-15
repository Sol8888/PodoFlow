
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class MovimientoRepositorio : IMovimientoRepositorio
{
    private readonly PodoFlowContext _context;

    public MovimientoRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public Task<List<Movimiento>> ObtenerTodosAsync(
        int? insumoId,
        long? atencionId,
        string? tipo,
        int pagina,
        int tamano)
    {
        IQueryable<Movimiento> consulta =
            _context.Movimientos.AsNoTracking();

        if (insumoId.HasValue)
        {
            consulta = consulta.Where(x =>
                x.InId == insumoId.Value);
        }

        if (atencionId.HasValue)
        {
            consulta = consulta.Where(x =>
                x.AtId == atencionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            consulta = consulta.Where(x => x.MoTipo == tipo);
        }

        return consulta
            .OrderByDescending(x => x.MoFechaHora)
            .ThenByDescending(x => x.MoId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public Task<Movimiento?> ObtenerPorIdAsync(long id)
    {
        return _context.Movimientos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MoId == id);
    }

    public Task<Insumo?> ObtenerInsumoAsync(int id)
    {
        return _context.Insumos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.InId == id);
    }

    public Task<bool> UsuarioActivoAsync(long id)
    {
        return _context.Usuarios.AnyAsync(x =>
            x.UsId == id && x.UsEstatus);
    }

    public Task<bool> AtencionExisteAsync(long id)
    {
        return _context.Atenciones
            .AnyAsync(x => x.AtId == id);
    }

    public Task<int> ActualizarStockAsync(
        int insumoId,
        byte[] versionOriginal,
        decimal stockAnterior,
        decimal stockNuevo,
        DateTime fechaModificacion)
    {
        return _context.Insumos
            .Where(x =>
                x.InId == insumoId &&
                x.InEstatus &&
                x.InVersion == versionOriginal &&
                x.InStockActu == stockAnterior)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    x => x.InStockActu,
                    stockNuevo)
                .SetProperty(
                    x => x.InFechaModi,
                    fechaModificacion));
    }

    public async Task AgregarAsync(Movimiento movimiento)
    {
        await _context.Movimientos.AddAsync(movimiento);
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }
}
