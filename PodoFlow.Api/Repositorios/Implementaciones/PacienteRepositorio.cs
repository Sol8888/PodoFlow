
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class PacienteRepositorio : IPacienteRepositorio
{
    private readonly PodoFlowContext _context;

    public PacienteRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public async Task<List<Paciente>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano)
    {
        var consulta = _context.Pacientes.AsNoTracking();

        if (!incluirInactivos)
            consulta = consulta.Where(x => x.PaEstatus);

        return await consulta
            .OrderBy(x => x.PaApelli)
            .ThenBy(x => x.PaNombres)
            .ThenBy(x => x.PaId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public async Task<Paciente?> ObtenerPorIdAsync(long id)
    {
        return await _context.Pacientes
            .FirstOrDefaultAsync(x => x.PaId == id);
    }

    public async Task<Paciente?> ObtenerPorIdentificacionAsync(
        string tipo, string identificacion)
    {
        return await _context.Pacientes
            .FirstOrDefaultAsync(x =>
                x.PaTipoIdent == tipo &&
                x.PaIdent == identificacion);
    }

    public async Task<Paciente> CrearAsync(Paciente paciente)
    {
        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();
        return paciente;
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void EstablecerVersionOriginal(
        Paciente paciente, byte[] version)
    {
        _context.Entry(paciente)
            .Property(x => x.PaVersion)
            .OriginalValue = version;
    }
}
