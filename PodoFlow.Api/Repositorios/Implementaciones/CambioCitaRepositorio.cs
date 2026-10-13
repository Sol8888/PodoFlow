
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class CambioCitaRepositorio : ICambioCitaRepositorio
{
    private readonly PodoFlowContext _context;

    public CambioCitaRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public async Task<CambioCita> CrearAsync(CambioCita cambio)
    {
        _context.CambiosCita.Add(cambio);
        await _context.SaveChangesAsync();
        return cambio;
    }

    public Task<CambioCita?> ObtenerPorIdAsync(long id)
    {
        return _context.CambiosCita
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CaId == id);
    }

    public async Task<List<CambioCita>> ObtenerTodosAsync(
        long? citaId,
        int pagina,
        int tamano)
    {
        var consulta = _context.CambiosCita.AsNoTracking();

        if (citaId.HasValue)
        {
            consulta = consulta.Where(x =>
                x.CiId == citaId.Value);
        }

        return await consulta
            .OrderByDescending(x => x.CaFechaCamb)
            .ThenByDescending(x => x.CaId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }
}
