
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.DTOs.Evolucion;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class EvolucionServicio : IEvolucionServicio
{
    private readonly IEvolucionRepositorio _repo;
    private readonly IEventoAuditoriaServicio _auditoria;
    private readonly PodoFlowContext _context;

    public EvolucionServicio(
        IEvolucionRepositorio repo,
        IEventoAuditoriaServicio auditoria,
        PodoFlowContext context)
    {
        _repo = repo;
        _auditoria = auditoria;
        _context = context;
    }

    public async Task<List<EvolucionRespuestaDto>> ObtenerTodosAsync(
        long? atencionId,
        bool incluirInactivos,
        int pagina,
        int tamano)
    {
        if (pagina < 1 || tamano < 1 || tamano > 100)
            throw new ArgumentException("Paginación inválida.");

        if (atencionId.HasValue && atencionId.Value <= 0)
            throw new ArgumentException("Atención inválida.");

        var evoluciones = await _repo.ObtenerTodosAsync(
            atencionId, incluirInactivos, pagina, tamano);

        return evoluciones.Select(Mapear).ToList();
    }

    public async Task<EvolucionRespuestaDto?> ObtenerPorIdAsync(long id)
    {
        var evolucion = await _repo.ObtenerPorIdAsync(id);

        return evolucion == null ? null : Mapear(evolucion);
    }

    public async Task<EvolucionRespuestaDto> CrearAsync(
        EvolucionCrearDto dto)
    {
        ValidarDescripcion(dto.Descripcion);

        if (dto.UsuarioId <= 0)
            throw new ArgumentException("Usuario inválido.");

        var descripcion = dto.Descripcion.Trim();

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var (atencion, historia) =
            await ValidarAtencionAbiertaAsync(dto.AtencionId);

        if (!await _repo.UsuarioActivoAsync(dto.UsuarioId))
            throw new ArgumentException(
                "El usuario no existe o está inactivo.");

        var existente = await _repo.BuscarIgualAsync(
            dto.AtencionId,
            dto.UsuarioId,
            descripcion);

        if (existente != null)
        {
            if (existente.EvEstatus)
                throw new InvalidOperationException(
                    "Ya existe una evolución activa idéntica.");

            existente.EvEstatus = true;

            await _repo.GuardarCambiosAsync();

            await AuditarAsync(
                existente, historia.PaId, "REACTIVAR");

            await tx.CommitAsync();

            return Mapear(existente);
        }

        var evolucion = new Evolucion
        {
            AtId = atencion.AtId,
            UsId = dto.UsuarioId,
            EvFecha = DateTime.UtcNow,
            EvDescri = descripcion,
            EvEstatus = true
        };

        var creada = await _repo.CrearAsync(evolucion);

        await AuditarAsync(
            creada, historia.PaId, "CREAR");

        await tx.CommitAsync();

        return Mapear(creada);
    }

    public async Task<bool> ActualizarAsync(
        long id, EvolucionActualizarDto dto)
    {
        ValidarDescripcion(dto.Descripcion);

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var evolucion = await _repo.ObtenerPorIdAsync(id);

        if (evolucion == null)
            return false;

        if (!evolucion.EvEstatus)
            throw new InvalidOperationException(
                "No se puede actualizar una evolución inactiva.");

        var (_, historia) =
            await ValidarAtencionAbiertaAsync(evolucion.AtId);

        var descripcion = dto.Descripcion.Trim();

        var duplicado = await _repo.BuscarIgualAsync(
            evolucion.AtId,
            evolucion.UsId,
            descripcion);

        if (duplicado != null && duplicado.EvId != id)
            throw new InvalidOperationException(
                "Ya existe una evolución con esa descripción.");

        evolucion.EvDescri = descripcion;

        await _repo.GuardarCambiosAsync();

        await AuditarAsync(
            evolucion, historia.PaId, "ACTUALIZAR");

        await tx.CommitAsync();

        return true;
    }

    public async Task<bool> EliminarAsync(long id)
    {
        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var evolucion = await _repo.ObtenerPorIdAsync(id);

        if (evolucion == null)
            return false;

        var (_, historia) =
            await ValidarAtencionAbiertaAsync(evolucion.AtId);

        if (!evolucion.EvEstatus)
            return true;

        evolucion.EvEstatus = false;

        await _repo.GuardarCambiosAsync();

        await AuditarAsync(
            evolucion, historia.PaId, "BAJA_LOGICA");

        await tx.CommitAsync();

        return true;
    }

    public async Task<bool> ReactivarAsync(long id)
    {
        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var evolucion = await _repo.ObtenerPorIdAsync(id);

        if (evolucion == null)
            return false;

        var (_, historia) =
            await ValidarAtencionAbiertaAsync(evolucion.AtId);

        if (!await _repo.UsuarioActivoAsync(evolucion.UsId))
            throw new InvalidOperationException(
                "El usuario responsable está inactivo.");

        if (evolucion.EvEstatus)
            return true;

        var duplicado = await _repo.BuscarIgualAsync(
            evolucion.AtId,
            evolucion.UsId,
            evolucion.EvDescri);

        if (duplicado != null &&
            duplicado.EvId != id &&
            duplicado.EvEstatus)
        {
            throw new InvalidOperationException(
                "Ya existe una evolución activa idéntica.");
        }

        evolucion.EvEstatus = true;

        await _repo.GuardarCambiosAsync();

        await AuditarAsync(
            evolucion, historia.PaId, "REACTIVAR");

        await tx.CommitAsync();

        return true;
    }

    private async Task<(Atencion, Historia)>
        ValidarAtencionAbiertaAsync(long atencionId)
    {
        var atencion = await _repo.ObtenerAtencionAsync(
            atencionId);

        if (atencion == null)
            throw new ArgumentException(
                "La atención no existe.");

        if (atencion.AtEstad != "ABIERTA")
            throw new InvalidOperationException(
                "La atención está cerrada o bloqueada.");

        var historia = await _repo.ObtenerHistoriaAsync(
            atencion.HiId);

        if (historia == null)
            throw new ArgumentException(
                "La historia clínica no existe.");

        if (historia.HiEstad != "ACTIVA")
            throw new InvalidOperationException(
                "La historia clínica está bloqueada o cerrada.");

        if (!await _repo.PacienteActivoAsync(historia.PaId))
            throw new InvalidOperationException(
                "El paciente está inactivo.");

        return (atencion, historia);
    }

    private static void ValidarDescripcion(string? descripcion)
    {
        if (string.IsNullOrWhiteSpace(descripcion) ||
            descripcion.Trim().Length > 3000)
        {
            throw new ArgumentException(
                "La descripción es obligatoria, máximo 3000 caracteres.");
        }
    }

    private async Task AuditarAsync(
        Evolucion evolucion,
        long pacienteId,
        string accion)
    {
        await _auditoria.RegistrarAsync(
            us_id: null,
            pa_id: pacienteId,
            ev_modulo: "CLINICA",
            ev_accion: accion,
            ev_entid: "EVOLUCION",
            ev_entid_id: evolucion.EvId.ToString(),
            ev_resul: "CORRECTO",
            ev_direc_ip: null,
            ev_agent_usua: null,
            ev_metad_json: null
        );
    }

    private static EvolucionRespuestaDto Mapear(Evolucion e)
    {
        return new EvolucionRespuestaDto
        {
            Id = e.EvId,
            AtencionId = e.AtId,
            UsuarioId = e.UsId,
            Fecha = e.EvFecha,
            Descripcion = e.EvDescri,
            Estatus = e.EvEstatus
        };
    }
}
