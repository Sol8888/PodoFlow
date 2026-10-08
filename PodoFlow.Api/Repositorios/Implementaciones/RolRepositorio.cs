
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class RolRepositorio : IRolRepositorio
{
    private readonly PodoFlowContext _context;

    public RolRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public async Task<List<Rol>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano)
    {
        var consulta = _context.Roles.AsNoTracking();

        if (!incluirInactivos)
        {
            consulta = consulta.Where(r => r.RoEstatus);
        }

        return await consulta
            .OrderBy(r => r.RoId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public async Task<Rol?> ObtenerPorIdAsync(int id)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.RoId == id);
    }

    public async Task<Rol?> ObtenerPorCodigoAsync(string codigo)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.RoCodigo == codigo);
    }

    public async Task<Rol> CrearAsync(Rol rol)
    {
        _context.Roles.Add(rol);
        await _context.SaveChangesAsync();
        return rol;
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }
}
