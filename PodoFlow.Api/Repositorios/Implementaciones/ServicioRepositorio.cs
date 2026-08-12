using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class ServicioRepositorio : IServicioRepositorio
{
    private readonly PodoFlowContext _context;

    public ServicioRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public async Task<List<Servicio>> ObtenerTodosAsync()
    {
        return await _context.Servicios
            .AsNoTracking()
            .OrderBy(x => x.SeNombre)
            .ToListAsync();
    }

    public async Task<Servicio?> ObtenerPorIdAsync(int id)
    {
        return await _context.Servicios
            .FirstOrDefaultAsync(x => x.SeId == id);
    }

    public async Task<Servicio> CrearAsync(Servicio servicio)
    {
        _context.Servicios.Add(servicio);

        await _context.SaveChangesAsync();

        return servicio;
    }

    public async Task ActualizarAsync(Servicio servicio)
    {
        _context.Servicios.Update(servicio);

        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(Servicio servicio)
    {
        servicio.SeEstatus = false;
        servicio.SeFechaModi = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}
