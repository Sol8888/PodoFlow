
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class RolPermisoRepositorio : IRolPermisoRepositorio
{
    private readonly PodoFlowContext _context;

    public RolPermisoRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public async Task<List<RolPermiso>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano)
    {
        var consulta = _context.RolesPermisos.AsNoTracking();

        if (!incluirInactivos)
            consulta = consulta.Where(x => x.RpEstatus);

        return await consulta
            .OrderBy(x => x.RoId)
            .ThenBy(x => x.PeId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public async Task<RolPermiso?> ObtenerPorIdAsync(
        int rolId, int permisoId)
    {
        return await _context.RolesPermisos
            .FirstOrDefaultAsync(x =>
                x.RoId == rolId &&
                x.PeId == permisoId);
    }

    public async Task<bool> RolActivoAsync(int rolId)
    {
        return await _context.Roles.AnyAsync(x =>
            x.RoId == rolId && x.RoEstatus);
    }

    public async Task<bool> PermisoActivoAsync(int permisoId)
    {
        return await _context.Permisos.AnyAsync(x =>
            x.PeId == permisoId && x.PeEstatus);
    }

    public async Task<RolPermiso> CrearAsync(RolPermiso relacion)
    {
        _context.RolesPermisos.Add(relacion);
        await _context.SaveChangesAsync();
        return relacion;
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task CambiarPermisoAsync(
        RolPermiso anterior,
        RolPermiso? nueva,
        int nuevoPermisoId)
    {
        await using var transaccion =
            await _context.Database.BeginTransactionAsync();

        try
        {
            if (!anterior.RpEstatus)
                throw new InvalidOperationException(
                    "La asignación anterior está inactiva.");

            // Se desactiva la relación anterior.
            anterior.RpEstatus = false;

            if (nueva != null)
            {
                // La relación ya existía: reactivar.
                nueva.RpEstatus = true;
            }
            else
            {
                // Nueva combinación rol-permiso.
                _context.RolesPermisos.Add(new RolPermiso
                {
                    RoId = anterior.RoId,
                    PeId = nuevoPermisoId,
                    RpFechaAsig = DateTime.UtcNow,
                    RpEstatus = true
                });
            }

            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();
        }
        catch
        {
            await transaccion.RollbackAsync();
            throw;
        }
    }
}
