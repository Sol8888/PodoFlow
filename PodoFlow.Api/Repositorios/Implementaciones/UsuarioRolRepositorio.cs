
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class UsuarioRolRepositorio : IUsuarioRolRepositorio
{
    private readonly PodoFlowContext _context;

    public UsuarioRolRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public async Task<List<UsuarioRol>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano)
    {
        var consulta = _context.UsuariosRoles.AsNoTracking();

        if (!incluirInactivos)
        {
            consulta = consulta.Where(x => x.UrEstatus);
        }

        return await consulta
            .OrderBy(x => x.UsId)
            .ThenBy(x => x.RoId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public async Task<UsuarioRol?> ObtenerPorIdAsync(
        long usuarioId, int rolId)
    {
        return await _context.UsuariosRoles
            .FirstOrDefaultAsync(x =>
                x.UsId == usuarioId &&
                x.RoId == rolId);
    }

    public async Task<bool> UsuarioActivoAsync(long usuarioId)
    {
        return await _context.Usuarios
            .AnyAsync(x =>
                x.UsId == usuarioId &&
                x.UsEstatus);
    }

    public async Task<bool> RolActivoAsync(int rolId)
    {
        return await _context.Roles
            .AnyAsync(x =>
                x.RoId == rolId &&
                x.RoEstatus);
    }

    public async Task<UsuarioRol> CrearAsync(UsuarioRol usuarioRol)
    {
        _context.UsuariosRoles.Add(usuarioRol);
        await _context.SaveChangesAsync();

        return usuarioRol;
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task CambiarRolAsync(
        UsuarioRol asignacionActual,
        UsuarioRol? asignacionNueva,
        int nuevoRolId)
    {
        await using var transaccion =
            await _context.Database.BeginTransactionAsync();

        try
        {
            if (!asignacionActual.UrEstatus)
            {
                throw new InvalidOperationException(
                    "La asignación original está inactiva.");
            }

            // Desactivar la asignación anterior.
            asignacionActual.UrEstatus = false;

            if (asignacionNueva != null)
            {
                // Reactivar una relación existente.
                asignacionNueva.UrEstatus = true;
            }
            else
            {
                // Crear la nueva relación.
                var nueva = new UsuarioRol
                {
                    UsId = asignacionActual.UsId,
                    RoId = nuevoRolId,
                    UrFechaAsig = DateTime.UtcNow,
                    UrEstatus = true
                };

                _context.UsuariosRoles.Add(nueva);
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
