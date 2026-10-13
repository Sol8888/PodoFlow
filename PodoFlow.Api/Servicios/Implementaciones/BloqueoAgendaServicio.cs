
using PodoFlow.Api.Datos;
using PodoFlow.Api.DTOs.BloqueoAgenda;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class BloqueoAgendaServicio : IBloqueoAgendaServicio
{
    private readonly IBloqueoAgendaRepositorio _repo;
    private readonly IEventoAuditoriaServicio _auditoria;
    private readonly PodoFlowContext _context;

    public BloqueoAgendaServicio(
        IBloqueoAgendaRepositorio repo,
        IEventoAuditoriaServicio auditoria,
        PodoFlowContext context)
    {
        _repo = repo;
        _auditoria = auditoria;
        _context = context;
    }

    public async Task<List<BloqueoAgendaRespuestaDto>>
        ObtenerTodosAsync(
            bool incluirInactivos,
            long? usuarioId,
            int pagina,
            int tamano)
    {
        var bloqueos = await _repo.ObtenerTodosAsync(
            incluirInactivos, usuarioId, pagina, tamano);

        return bloqueos.Select(Mapear).ToList();
    }

    public async Task<BloqueoAgendaRespuestaDto?>
        ObtenerPorIdAsync(long id)
    {
        var bloqueo = await _repo.ObtenerPorIdAsync(id);

        return bloqueo == null ? null : Mapear(bloqueo);
    }

    public async Task<BloqueoAgendaRespuestaDto> CrearAsync(
        BloqueoAgendaCrearDto dto)
    {
        await ValidarAsync(dto);

        var inicio = dto.FechaInicio;
        var fin = dto.FechaFin;
        var motivo = dto.Motivo.Trim();

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var existente = await _repo.BuscarIgualAsync(
            dto.UsuarioId, inicio, fin, motivo);

        if (existente != null && existente.BlEstatus)
            throw new InvalidOperationException(
                "El bloqueo ya está registrado y activo.");

        if (await _repo.ExisteSolapamientoAsync(
            dto.UsuarioId, inicio, fin,
            existente?.BlId))
            throw new InvalidOperationException(
                "Ya existe un bloqueo activo en ese horario.");

        BloqueoAgenda bloqueo;
        string accion;

        if (existente != null)
        {
            existente.BlEstatus = true;
            bloqueo = existente;
            accion = "REACTIVAR";
            await _repo.GuardarCambiosAsync();
        }
        else
        {
            bloqueo = new BloqueoAgenda
            {
                UsId = dto.UsuarioId,
                BlFechaIni = inicio,
                BlFechaFin = fin,
                BlMotivo = motivo,
                BlFechaCrea = DateTime.UtcNow,
                BlEstatus = true
            };

            await _repo.CrearAsync(bloqueo);
            accion = "CREAR";
        }

        await RegistrarAuditoriaAsync(bloqueo, accion);
        await tx.CommitAsync();

        return Mapear(bloqueo);
    }

    public async Task<bool> ActualizarAsync(
        long id, BloqueoAgendaActualizarDto dto)
    {
        await ValidarAsync(dto);

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var bloqueo = await _repo.ObtenerPorIdAsync(id);
        if (bloqueo == null) return false;

        if (!bloqueo.BlEstatus)
            throw new InvalidOperationException(
                "No puedes actualizar un bloqueo inactivo.");

        if (await _repo.ExisteSolapamientoAsync(
            dto.UsuarioId,
            dto.FechaInicio,
            dto.FechaFin,
            id))
            throw new InvalidOperationException(
                "El horario se superpone con otro bloqueo.");

        bloqueo.UsId = dto.UsuarioId;
        bloqueo.BlFechaIni = dto.FechaInicio;
        bloqueo.BlFechaFin = dto.FechaFin;
        bloqueo.BlMotivo = dto.Motivo.Trim();

        await _repo.GuardarCambiosAsync();
        await RegistrarAuditoriaAsync(bloqueo, "ACTUALIZAR");
        await tx.CommitAsync();

        return true;
    }

    public async Task<bool> EliminarAsync(long id)
    {
        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var bloqueo = await _repo.ObtenerPorIdAsync(id);
        if (bloqueo == null) return false;

        if (!bloqueo.BlEstatus) return true;

        bloqueo.BlEstatus = false;

        await _repo.GuardarCambiosAsync();
        await RegistrarAuditoriaAsync(bloqueo, "BAJA_LOGICA");
        await tx.CommitAsync();

        return true;
    }

    public async Task<bool> ReactivarAsync(long id)
    {
        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var bloqueo = await _repo.ObtenerPorIdAsync(id);
        if (bloqueo == null) return false;

        if (bloqueo.BlEstatus) return true;

        if (bloqueo.UsId.HasValue &&
            !await _repo.UsuarioActivoAsync(bloqueo.UsId.Value))
            throw new ArgumentException(
                "El usuario no existe o está inactivo.");

        if (await _repo.ExisteSolapamientoAsync(
            bloqueo.UsId,
            bloqueo.BlFechaIni,
            bloqueo.BlFechaFin,
            id))
            throw new InvalidOperationException(
                "No se puede reactivar: existe otro bloqueo superpuesto.");

        bloqueo.BlEstatus = true;

        await _repo.GuardarCambiosAsync();
        await RegistrarAuditoriaAsync(bloqueo, "REACTIVAR");
        await tx.CommitAsync();

        return true;
    }

    private async Task ValidarAsync(BloqueoAgendaCrearDto dto)
    {
        if (dto.FechaFin <= dto.FechaInicio)
            throw new ArgumentException(
                "La fecha final debe ser posterior a la inicial.");

        if (dto.FechaInicio == default ||
            dto.FechaFin == default)
            throw new ArgumentException(
                "Las fechas son obligatorias.");

        if (string.IsNullOrWhiteSpace(dto.Motivo) ||
            dto.Motivo.Trim().Length > 300)
            throw new ArgumentException(
                "El motivo es obligatorio y admite máximo 300 caracteres.");

        if (dto.UsuarioId.HasValue &&
            !await _repo.UsuarioActivoAsync(dto.UsuarioId.Value))
            throw new ArgumentException(
                "El usuario no existe o está inactivo.");
    }

    private async Task RegistrarAuditoriaAsync(
        BloqueoAgenda bloqueo, string accion)
    {
        // TODO: reemplazar NULL por el ID del usuario
        // obtenido del contexto de autenticación.
        await _auditoria.RegistrarAsync(
            us_id: null,
            pa_id: null,
            ev_modulo: "AGENDA",
            ev_accion: accion,
            ev_entid: "BLOQUEO_AGENDA",
            ev_entid_id: bloqueo.BlId.ToString(),
            ev_resul: "CORRECTO",
            ev_direc_ip: null,
            ev_agent_usua: null,
            ev_metad_json: null
        );
    }

    private static BloqueoAgendaRespuestaDto Mapear(
        BloqueoAgenda b)
    {
        return new BloqueoAgendaRespuestaDto
        {
            Id = b.BlId,
            UsuarioId = b.UsId,
            FechaInicio = b.BlFechaIni,
            FechaFin = b.BlFechaFin,
            Motivo = b.BlMotivo,
            FechaCreacion = b.BlFechaCrea,
            Estatus = b.BlEstatus
        };
    }
}
