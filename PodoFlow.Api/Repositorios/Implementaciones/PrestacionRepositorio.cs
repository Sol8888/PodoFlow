
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class PrestacionRepositorio : IPrestacionRepositorio
{
    private readonly PodoFlowContext _context;

    public PrestacionRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public Task<List<Prestacion>> ObtenerTodosAsync(
        long? pacienteId,
        string? estado,
        int pagina,
        int tamano)
    {
        IQueryable<Prestacion> consulta =
            _context.Prestaciones.AsNoTracking();

        if (pacienteId.HasValue)
            consulta = consulta.Where(x =>
                x.PaId == pacienteId.Value);

        if (!string.IsNullOrWhiteSpace(estado))
            consulta = consulta.Where(x =>
                x.PrEstad == estado);

        return consulta
            .OrderByDescending(x => x.PrFechaPres)
            .ThenByDescending(x => x.PrId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public Task<Prestacion?> ObtenerPorIdAsync(long id)
    {
        return _context.Prestaciones
            .FirstOrDefaultAsync(x => x.PrId == id);
    }

    public Task<bool> PacienteActivoAsync(long id)
    {
        return _context.Pacientes.AnyAsync(x =>
            x.PaId == id && x.PaEstatus);
    }

    public Task<bool> UsuarioActivoAsync(long id)
    {
        return _context.Usuarios.AnyAsync(x =>
            x.UsId == id && x.UsEstatus);
    }

    public Task<bool> ServicioExisteAsync(int id)
    {
        return _context.Servicios.AnyAsync(x => x.SeId == id);
    }

    public Task<Cita?> ObtenerCitaAsync(long id)
    {
        return _context.Citas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CiId == id);
    }

    public Task<Atencion?> ObtenerAtencionAsync(long id)
    {
        return _context.Atenciones
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.AtId == id);
    }

    public Task<Historia?> ObtenerHistoriaAsync(long id)
    {
        return _context.Historias
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.HiId == id);
    }

    public async Task AgregarAsync(Prestacion prestacion)
    {
        await _context.Prestaciones.AddAsync(prestacion);
    }

    public Task GuardarCambiosAsync()
    {
        return _context.SaveChangesAsync();
    }
}
