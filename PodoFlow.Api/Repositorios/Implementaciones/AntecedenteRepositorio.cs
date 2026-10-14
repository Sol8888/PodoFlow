
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class AntecedenteRepositorio : IAntecedenteRepositorio
{
    private readonly PodoFlowContext _context;

    public AntecedenteRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public async Task<List<Antecedente>> ObtenerTodosAsync(
        long? historiaId,
        bool incluirInactivos,
        int pagina,
        int tamano)
    {
        var consulta = _context.Antecedentes.AsNoTracking();

        if (historiaId.HasValue)
        {
            consulta = consulta.Where(x =>
                x.HiId == historiaId.Value);
        }

        if (!incluirInactivos)
        {
            consulta = consulta.Where(x => x.AnEstatus);
        }

        return await consulta
            .OrderByDescending(x => x.AnFechaReg)
            .ThenByDescending(x => x.AnId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public Task<Antecedente?> ObtenerPorIdAsync(long id)
    {
        return _context.Antecedentes
            .FirstOrDefaultAsync(x => x.AnId == id);
    }

    public Task<Antecedente?> BuscarIgualAsync(
        long historiaId,
        string tipo,
        string descripcion)
    {
        return _context.Antecedentes.FirstOrDefaultAsync(x =>
            x.HiId == historiaId &&
            x.AnTipo == tipo &&
            x.AnDescri == descripcion);
    }

    public Task<Historia?> ObtenerHistoriaAsync(long historiaId)
    {
        return _context.Historias
            .FirstOrDefaultAsync(x => x.HiId == historiaId);
    }

    public Task<bool> PacienteActivoAsync(long pacienteId)
    {
        return _context.Pacientes.AnyAsync(x =>
            x.PaId == pacienteId && x.PaEstatus);
    }

    public Task<bool> UsuarioActivoAsync(long usuarioId)
    {
        return _context.Usuarios.AnyAsync(x =>
            x.UsId == usuarioId && x.UsEstatus);
    }

    public async Task<Antecedente> CrearAsync(
        Antecedente antecedente)
    {
        _context.Antecedentes.Add(antecedente);
        await _context.SaveChangesAsync();

        return antecedente;
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }
}
