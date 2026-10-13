
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class PermisoRepositorio : IPermisoRepositorio
{
    private readonly PodoFlowContext _context;

    public PermisoRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public async Task<List<Permiso>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano)
    {
        var consulta = _context.Permisos.AsNoTracking();

        if (!incluirInactivos)
        {
            consulta = consulta.Where(p => p.PeEstatus);
        }

        return await consulta
            .OrderBy(p => p.PeId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public async Task<Permiso?> ObtenerPorIdAsync(int id)
    {
        return await _context.Permisos
            .FirstOrDefaultAsync(p => p.PeId == id);
    }

    public async Task<Permiso?> ObtenerPorCodigoAsync(string codigo)
    {
        return await _context.Permisos
            .FirstOrDefaultAsync(p => p.PeCodigo == codigo);
    }

    public async Task<Permiso> CrearAsync(Permiso permiso)
    {
        _context.Permisos.Add(permiso);
        await _context.SaveChangesAsync();

        return permiso;
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }
}
