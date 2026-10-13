
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class CitaRepositorio : ICitaRepositorio
{
    private readonly PodoFlowContext _context;

    private static readonly TimeZoneInfo ZonaEcuador =
        ObtenerZonaEcuador();

    public CitaRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    private static TimeZoneInfo ObtenerZonaEcuador()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(
                "America/Guayaquil");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(
                "SA Pacific Standard Time");
        }
    }

    public async Task<List<Cita>> ObtenerTodosAsync(
        long? pacienteId,
        long? usuarioId,
        string? estado,
        int pagina,
        int tamano)
    {
        var consulta = _context.Citas.AsNoTracking();

        if (pacienteId.HasValue)
            consulta = consulta.Where(x =>
                x.PaId == pacienteId.Value);

        if (usuarioId.HasValue)
            consulta = consulta.Where(x =>
                x.UsId == usuarioId.Value);

        if (!string.IsNullOrWhiteSpace(estado))
            consulta = consulta.Where(x =>
                x.CiEstad == estado);

        return await consulta
            .OrderByDescending(x => x.CiFechaIni)
            .ThenByDescending(x => x.CiId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public Task<Cita?> ObtenerPorIdAsync(long id)
    {
        return _context.Citas.FirstOrDefaultAsync(x =>
            x.CiId == id);
    }

    public Task<bool> PacienteActivoAsync(long id)
    {
        return _context.Pacientes.AnyAsync(x =>
            x.PaId == id && x.PaEstatus);
    }

    public Task<bool> ServicioActivoAsync(int id)
    {
        return _context.Servicios.AnyAsync(x =>
            x.SeId == id && x.SeEstatus);
    }

    public Task<bool> UsuarioActivoAsync(long id)
    {
        return _context.Usuarios.AnyAsync(x =>
            x.UsId == id && x.UsEstatus);
    }

    public async Task<bool> DisponibleAsync(
        long? usuarioId,
        DateTime inicioUtc,
        DateTime finUtc,
        long? excluirCitaId = null)
    {
        // 1. Comprobar bloqueos generales o del profesional.
        bool bloqueado = await _context.BloqueosAgenda
            .AsNoTracking()
            .AnyAsync(x =>
                x.BlEstatus &&
                (x.UsId == null ||
                 (usuarioId.HasValue &&
                  x.UsId == usuarioId.Value)) &&
                x.BlFechaIni < finUtc &&
                inicioUtc < x.BlFechaFin);

        if (bloqueado)
            return false;

        // 2. Comprobar citas superpuestas.
        bool ocupado = await _context.Citas
            .AsNoTracking()
            .AnyAsync(x =>
                (!excluirCitaId.HasValue ||
                 x.CiId != excluirCitaId.Value) &&
                x.CiEstad != "CANCELADA" &&
                x.CiEstad != "NO_ASISTE" &&
                (usuarioId == null ||
                 x.UsId == null ||
                 x.UsId == usuarioId) &&
                x.CiFechaIni < finUtc &&
                inicioUtc < x.CiFechaFin);

        if (ocupado)
            return false;

        // 3. Convertir UTC a hora local de Ecuador.
        var inicioLocal = TimeZoneInfo.ConvertTimeFromUtc(
            inicioUtc, ZonaEcuador);

        var finLocal = TimeZoneInfo.ConvertTimeFromUtc(
            finUtc, ZonaEcuador);

        // En esta versión las citas no cruzan medianoche local.
        if (inicioLocal.Date != finLocal.Date)
            return false;

        int diaSemana = ((int)inicioLocal.DayOfWeek + 6) % 7 + 1;

        var horaInicio = inicioLocal.TimeOfDay;
        var horaFin = finLocal.TimeOfDay;
        var fecha = inicioLocal.Date;

        // 4. Comprobar horario de atención.
        var horarios = await _context.HorariosAtencion
            .AsNoTracking()
            .Where(x =>
                x.HoEstatus &&
                x.HoDiaSem == diaSemana &&
                (x.UsId == usuarioId || x.UsId == null) &&
                (x.HoFechaDesd == null ||
                 x.HoFechaDesd <= fecha) &&
                (x.HoFechaHast == null ||
                 x.HoFechaHast >= fecha))
            .ToListAsync();

        return horarios.Any(x =>
            x.HoHoraIni <= horaInicio &&
            x.HoHoraFin >= horaFin);
    }

    public async Task<Cita> CrearAsync(Cita cita)
    {
        _context.Citas.Add(cita);
        await _context.SaveChangesAsync();
        return cita;
    }

    public Task GuardarCambiosAsync()
    {
        return _context.SaveChangesAsync();
    }

    public void EstablecerVersionOriginal(
        Cita cita, byte[] version)
    {
        _context.Entry(cita)
            .Property(x => x.CiVersion)
            .OriginalValue = version;
    }
}
