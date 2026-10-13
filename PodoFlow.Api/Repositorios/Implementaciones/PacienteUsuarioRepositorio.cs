
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class PacienteUsuarioRepositorio : IPacienteUsuarioRepositorio
{
    private readonly PodoFlowContext _context;

    public PacienteUsuarioRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public async Task<List<PacienteUsuario>> ListarAsync(
        bool incluirInactivos, int pagina, int tamano)
    {
        var query = _context.PacientesUsuarios.AsNoTracking();

        if (!incluirInactivos)
            query = query.Where(x => x.PuEstatus);

        return await query
            .OrderBy(x => x.PaId)
            .ThenBy(x => x.UsId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public Task<PacienteUsuario?> ObtenerAsync(
        long pacienteId, long usuarioId) =>
        _context.PacientesUsuarios.FirstOrDefaultAsync(x =>
            x.PaId == pacienteId && x.UsId == usuarioId);

    public Task<PacienteUsuario?> PorPacienteAsync(long pacienteId) =>
        _context.PacientesUsuarios.FirstOrDefaultAsync(x =>
            x.PaId == pacienteId);

    public Task<PacienteUsuario?> PorUsuarioAsync(long usuarioId) =>
        _context.PacientesUsuarios.FirstOrDefaultAsync(x =>
            x.UsId == usuarioId);

    public Task<bool> PacienteActivoAsync(long id) =>
        _context.Pacientes.AnyAsync(x =>
            x.PaId == id && x.PaEstatus);

    public Task<bool> UsuarioActivoAsync(long id) =>
        _context.Usuarios.AnyAsync(x =>
            x.UsId == id && x.UsEstatus);

    public async Task CrearAsync(PacienteUsuario relacion)
    {
        _context.PacientesUsuarios.Add(relacion);
        await _context.SaveChangesAsync();
    }

    public Task GuardarAsync() => _context.SaveChangesAsync();
}
