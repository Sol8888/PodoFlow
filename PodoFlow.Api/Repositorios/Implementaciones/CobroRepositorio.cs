
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class CobroRepositorio : ICobroRepositorio
{
    private readonly PodoFlowContext _context;

    public CobroRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public Task<List<Cobro>> ObtenerTodosAsync(
        long? prestacionId,
        string? estado,
        int pagina,
        int tamano)
    {
        IQueryable<Cobro> consulta =
            _context.Cobros.AsNoTracking();

        if (prestacionId.HasValue)
        {
            consulta = consulta.Where(x =>
                x.PrId == prestacionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            consulta = consulta.Where(x =>
                x.CoEstad == estado);
        }

        return consulta
            .OrderByDescending(x => x.CoFechaPago)
            .ThenByDescending(x => x.CoId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public Task<Cobro?> ObtenerPorIdAsync(long id)
    {
        return _context.Cobros
            .FirstOrDefaultAsync(x => x.CoId == id);
    }

    public Task<Cobro?> ObtenerPorIdSinSeguimientoAsync(long id)
    {
        return _context.Cobros
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CoId == id);
    }

    public Task<Prestacion?> BloquearPrestacionAsync(long id)
    {
        // Debe ejecutarse DENTRO de una transacción.
        // Todas las escrituras de COBRO bloquean
        // primero la fila de PRESTACION.
        return _context.Prestaciones
            .FromSqlInterpolated($"""
                SELECT *
                FROM fin.PRESTACION WITH (UPDLOCK, HOLDLOCK)
                WHERE pr_id = {id}
                """)
            .SingleOrDefaultAsync();
    }

    public Task<Prestacion?> ObtenerPrestacionAsync(long id)
    {
        return _context.Prestaciones
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PrId == id);
    }

    public Task<FormaPago?> ObtenerFormaPagoAsync(int id)
    {
        return _context.FormasPago
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FoId == id);
    }

    public Task<bool> UsuarioActivoAsync(long id)
    {
        return _context.Usuarios
            .AnyAsync(x => x.UsId == id && x.UsEstatus);
    }

    public async Task<decimal> TotalCobradoAsync(long prestacionId)
    {
        return await _context.Cobros
            .AsNoTracking()
            .Where(x =>
                x.PrId == prestacionId &&
                x.CoEstad == "REGISTRADO")
            .SumAsync(x => (decimal?)x.CoValor) ?? 0m;
    }

    public async Task AgregarAsync(Cobro cobro)
    {
        await _context.Cobros.AddAsync(cobro);
    }

    public Task GuardarCambiosAsync()
    {
        return _context.SaveChangesAsync();
    }
}
