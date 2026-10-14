
using PodoFlow.Api.DTOs.HorarioAtencion;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class HorarioAtencionServicio
    : IHorarioAtencionServicio
{
    private readonly IHorarioAtencionRepositorio _repo;

    public HorarioAtencionServicio(
        IHorarioAtencionRepositorio repo)
    {
        _repo = repo;
    }

    public async Task<List<HorarioAtencionRespuestaDto>>
        ObtenerTodosAsync(
            bool incluirInactivos,
            int pagina,
            int tamano)
    {
        var horarios = await _repo.ObtenerTodosAsync(
            incluirInactivos, pagina, tamano);

        return horarios.Select(Mapear).ToList();
    }

    public async Task<HorarioAtencionRespuestaDto?>
        ObtenerPorIdAsync(int id)
    {
        var horario = await _repo.ObtenerPorIdAsync(id);

        return horario == null ? null : Mapear(horario);
    }

    public async Task<HorarioAtencionRespuestaDto> CrearAsync(
        HorarioAtencionCrearDto dto)
    {
        await ValidarAsync(dto);

        var horario = new HorarioAtencion();

        CopiarDatos(horario, dto);
        horario.HoEstatus = true;

        if (await _repo.ExisteSolapamientoAsync(horario))
            throw new InvalidOperationException(
                "Ya existe un horario activo que se superpone.");

        var creado = await _repo.CrearAsync(horario);

        return Mapear(creado);
    }

    public async Task<bool> ActualizarAsync(
        int id,
        HorarioAtencionActualizarDto dto)
    {
        var horario = await _repo.ObtenerPorIdAsync(id);

        if (horario == null)
            return false;

        if (!horario.HoEstatus)
            throw new InvalidOperationException(
                "No se puede actualizar un horario inactivo.");

        await ValidarAsync(dto);

        var candidato = new HorarioAtencion();
        CopiarDatos(candidato, dto);

        if (await _repo.ExisteSolapamientoAsync(
            candidato, id))
            throw new InvalidOperationException(
                "El horario se superpone con otro existente.");

        CopiarDatos(horario, dto);

        await _repo.GuardarCambiosAsync();

        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var horario = await _repo.ObtenerPorIdAsync(id);

        if (horario == null)
            return false;

        if (!horario.HoEstatus)
            return true;

        horario.HoEstatus = false;

        await _repo.GuardarCambiosAsync();

        return true;
    }

    public async Task<bool> ReactivarAsync(int id)
    {
        var horario = await _repo.ObtenerPorIdAsync(id);

        if (horario == null)
            return false;

        if (horario.HoEstatus)
            return true;

        if (horario.UsId.HasValue &&
            !await _repo.UsuarioActivoAsync(
                horario.UsId.Value))
            throw new ArgumentException(
                "No se puede reactivar: el usuario está inactivo.");

        if (await _repo.ExisteSolapamientoAsync(
            horario, id))
            throw new InvalidOperationException(
                "No se puede reactivar porque existe un horario superpuesto.");

        horario.HoEstatus = true;

        await _repo.GuardarCambiosAsync();

        return true;
    }

    private async Task ValidarAsync(
        HorarioAtencionCrearDto dto)
    {
        if (dto.DiaSemana < 1 || dto.DiaSemana > 7)
            throw new ArgumentException(
                "El día debe estar entre 1 y 7.");

        if (!dto.HoraInicio.HasValue ||
            !dto.HoraFin.HasValue)
            throw new ArgumentException(
                "Las horas son obligatorias.");

        var inicio = dto.HoraInicio.Value;
        var fin = dto.HoraFin.Value;

        if (inicio < TimeSpan.Zero ||
            inicio >= TimeSpan.FromDays(1) ||
            fin <= TimeSpan.Zero ||
            fin >= TimeSpan.FromDays(1) ||
            fin <= inicio)
            throw new ArgumentException(
                "El rango de horas no es válido.");

        if (dto.FechaDesde.HasValue &&
            dto.FechaHasta.HasValue &&
            dto.FechaHasta.Value.Date <
                dto.FechaDesde.Value.Date)
            throw new ArgumentException(
                "La fecha final no puede ser anterior a la inicial.");

        if (dto.UsuarioId.HasValue)
        {
            if (dto.UsuarioId.Value <= 0 ||
                !await _repo.UsuarioActivoAsync(
                    dto.UsuarioId.Value))
                throw new ArgumentException(
                    "El usuario no existe o está inactivo.");
        }
    }

    private static void CopiarDatos(
        HorarioAtencion horario,
        HorarioAtencionCrearDto dto)
    {
        horario.UsId = dto.UsuarioId;
        horario.HoDiaSem = dto.DiaSemana;
        horario.HoHoraIni = dto.HoraInicio!.Value;
        horario.HoHoraFin = dto.HoraFin!.Value;
        horario.HoFechaDesd = dto.FechaDesde?.Date;
        horario.HoFechaHast = dto.FechaHasta?.Date;
    }

    private static HorarioAtencionRespuestaDto Mapear(
        HorarioAtencion h)
    {
        return new HorarioAtencionRespuestaDto
        {
            Id = h.HoId,
            UsuarioId = h.UsId,
            DiaSemana = h.HoDiaSem,
            HoraInicio = h.HoHoraIni,
            HoraFin = h.HoHoraFin,
            FechaDesde = h.HoFechaDesd,
            FechaHasta = h.HoFechaHast,
            Estatus = h.HoEstatus
        };
    }
}
