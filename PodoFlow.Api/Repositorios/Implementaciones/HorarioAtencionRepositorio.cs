
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class HorarioAtencionRepositorio
    : IHorarioAtencionRepositorio
{
    private readonly PodoFlowContext _context;

    public HorarioAtencionRepositorio(
        PodoFlowContext context)
    {
        _context = context;
    }

    public async Task<List<HorarioAtencion>> ObtenerTodosAsync(
        bool incluirInactivos,
        int pagina,
        int tamano)
    {
        var consulta = _context.HorariosAtencion
            .AsNoTracking();

        if (!incluirInactivos)
            consulta = consulta.Where(x => x.HoEstatus);

        return await consulta
            .OrderBy(x => x.HoDiaSem)
            .ThenBy(x => x.HoHoraIni)
            .ThenBy(x => x.HoId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public Task<HorarioAtencion?> ObtenerPorIdAsync(int id)
    {
        return _context.HorariosAtencion
            .FirstOrDefaultAsync(x => x.HoId == id);
    }

    public Task<bool> UsuarioActivoAsync(long usuarioId)
    {
        return _context.Usuarios.AnyAsync(x =>
            x.UsId == usuarioId && x.UsEstatus);
    }

    public async Task<bool> ExisteSolapamientoAsync(
        HorarioAtencion horario,
        int? excluirId = null)
    {
        return await _context.HorariosAtencion
            .AsNoTracking()
            .AnyAsync(x =>
                x.HoEstatus &&
                (!excluirId.HasValue ||
                    x.HoId != excluirId.Value) &&
                x.UsId == horario.UsId &&
                x.HoDiaSem == horario.HoDiaSem &&

                // Intersección de horas
                x.HoHoraIni < horario.HoHoraFin &&
                horario.HoHoraIni < x.HoHoraFin &&

                // Intersección de rangos de fechas
                (x.HoFechaHast == null ||
                    horario.HoFechaDesd == null ||
                    x.HoFechaHast >= horario.HoFechaDesd) &&

                (horario.HoFechaHast == null ||
                    x.HoFechaDesd == null ||
                    horario.HoFechaHast >= x.HoFechaDesd));
    }

    public async Task<HorarioAtencion> CrearAsync(
        HorarioAtencion horario)
    {
        _context.HorariosAtencion.Add(horario);

        await _context.SaveChangesAsync();

        return horario;
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }
}
