
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class IntentoAccesoRepositorio : IIntentoAccesoRepositorio
{
    private readonly PodoFlowContext _context;

    public IntentoAccesoRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public async Task<IntentoAcceso> CrearAsync(
        IntentoAcceso intento)
    {
        _context.IntentosAcceso.Add(intento);

        await _context.SaveChangesAsync();

        return intento;
    }

    public async Task<IntentoAcceso?> ObtenerPorIdAsync(long id)
    {
        return await _context.IntentosAcceso
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.InId == id);
    }

    public async Task<List<IntentoAcceso>> ObtenerTodosAsync(
        long? usuarioId,
        bool? exitoso,
        int pagina,
        int tamano)
    {
        var consulta = _context.IntentosAcceso.AsNoTracking();

        if (usuarioId.HasValue)
        {
            consulta = consulta.Where(x =>
                x.UsId == usuarioId.Value);
        }

        if (exitoso.HasValue)
        {
            consulta = consulta.Where(x =>
                x.InExitoso == exitoso.Value);
        }

        return await consulta
            .OrderByDescending(x => x.InFechaHora)
            .ThenByDescending(x => x.InId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }
}
