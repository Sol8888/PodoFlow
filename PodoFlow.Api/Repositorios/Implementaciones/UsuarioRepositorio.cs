
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;


namespace PodoFlow.Api.Repositorios.Implementaciones;

public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly PodoFlowContext _context;

    public UsuarioRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public async Task<List<Usuario>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano)
    {
        var consulta = _context.Usuarios.AsNoTracking();

        if (!incluirInactivos)
            consulta = consulta.Where(x => x.UsEstatus);

        return await consulta
            .OrderBy(x => x.UsNombre)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public async Task<Usuario?> ObtenerPorIdAsync(long id)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(x => x.UsId == id);
    }

    public async Task<Usuario?> ObtenerPorCorreoAsync(string correo)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(x => x.UsCorreo == correo);
    }

    public async Task<Usuario> CrearAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }

}
