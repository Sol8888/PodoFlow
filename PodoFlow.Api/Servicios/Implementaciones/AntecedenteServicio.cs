
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.DTOs.Antecedente;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class AntecedenteServicio : IAntecedenteServicio
{
    private readonly IAntecedenteRepositorio _repo;
    private readonly IEventoAuditoriaServicio _auditoria;
    private readonly PodoFlowContext _context;

    public AntecedenteServicio(
        IAntecedenteRepositorio repo,
        IEventoAuditoriaServicio auditoria,
        PodoFlowContext context)
    {
        _repo = repo;
        _auditoria = auditoria;
        _context = context;
    }

    public async Task<List<AntecedenteRespuestaDto>> ObtenerTodosAsync(
        long? historiaId,
        bool incluirInactivos,
        int pagina,
        int tamano)
    {
        if (pagina < 1 || tamano < 1 || tamano > 100)
            throw new ArgumentException("Paginación inválida.");

        if (historiaId.HasValue && historiaId.Value <= 0)
            throw new ArgumentException("Historia inválida.");

        var datos = await _repo.ObtenerTodosAsync(
            historiaId, incluirInactivos, pagina, tamano);

        return datos.Select(Mapear).ToList();
    }

    public async Task<AntecedenteRespuestaDto?> ObtenerPorIdAsync(
        long id)
    {
        var antecedente = await _repo.ObtenerPorIdAsync(id);

        return antecedente == null
            ? null
            : Mapear(antecedente);
    }

    public async Task<AntecedenteRespuestaDto> CrearAsync(
        AntecedenteCrearDto dto)
    {
        ValidarDatos(dto.Tipo, dto.Descripcion);

        var tipo = dto.Tipo.Trim().ToUpperInvariant();
        var descripcion = dto.Descripcion.Trim();

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var historia = await ValidarHistoriaActivaAsync(
            dto.HistoriaId);

        await ValidarUsuarioAsync(dto.UsuarioId);

        // Evitar duplicados exactos dentro de la misma historia.
        var existente = await _repo.BuscarIgualAsync(
            dto.HistoriaId, tipo, descripcion);

        if (existente != null)
        {
            if (existente.AnEstatus)
            {
                throw new InvalidOperationException(
                    "Este antecedente ya está activo en la historia.");
            }

            existente.AnEstatus = true;

            await _repo.GuardarCambiosAsync();

            await AuditarAsync(
                existente, historia.PaId, "REACTIVAR");

            await tx.CommitAsync();

            return Mapear(existente);
        }

        var antecedente = new Antecedente
        {
            HiId = dto.HistoriaId,
            UsId = dto.UsuarioId,
            AnTipo = tipo,
            AnDescri = descripcion,
            AnFechaReg = DateTime.UtcNow,
            AnEstatus = true
        };

        var creado = await _repo.CrearAsync(antecedente);

        await AuditarAsync(
            creado, historia.PaId, "CREAR");

        await tx.CommitAsync();

        return Mapear(creado);
    }

    public async Task<bool> ActualizarAsync(
        long id,
        AntecedenteActualizarDto dto)
    {
        ValidarDatos(dto.Tipo, dto.Descripcion);

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var antecedente = await _repo.ObtenerPorIdAsync(id);

        if (antecedente == null)
            return false;

        if (!antecedente.AnEstatus)
        {
            throw new InvalidOperationException(
                "No se puede actualizar un antecedente inactivo.");
        }

        var historia = await ValidarHistoriaActivaAsync(
            antecedente.HiId);

        var tipo = dto.Tipo.Trim().ToUpperInvariant();
        var descripcion = dto.Descripcion.Trim();

        var duplicado = await _repo.BuscarIgualAsync(
            antecedente.HiId, tipo, descripcion);

        if (duplicado != null && duplicado.AnId != id)
        {
            throw new InvalidOperationException(
                "Ya existe ese antecedente en la historia.");
        }

        antecedente.AnTipo = tipo;
        antecedente.AnDescri = descripcion;

        await _repo.GuardarCambiosAsync();

        await AuditarAsync(
            antecedente, historia.PaId, "ACTUALIZAR");

        await tx.CommitAsync();

        return true;
    }

    public async Task<bool> EliminarAsync(long id)
    {
        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var antecedente = await _repo.ObtenerPorIdAsync(id);

        if (antecedente == null)
            return false;

        var historia = await ValidarHistoriaActivaAsync(
            antecedente.HiId);

        if (!antecedente.AnEstatus)
            return true;

        antecedente.AnEstatus = false;

        await _repo.GuardarCambiosAsync();

        await AuditarAsync(
            antecedente, historia.PaId, "BAJA_LOGICA");

        await tx.CommitAsync();

        return true;
    }

    public async Task<bool> ReactivarAsync(long id)
    {
        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var antecedente = await _repo.ObtenerPorIdAsync(id);

        if (antecedente == null)
            return false;

        var historia = await ValidarHistoriaActivaAsync(
            antecedente.HiId);

        if (antecedente.AnEstatus)
            return true;

        var duplicado = await _repo.BuscarIgualAsync(
            antecedente.HiId,
            antecedente.AnTipo,
            antecedente.AnDescri);

        if (duplicado != null &&
            duplicado.AnId != id &&
            duplicado.AnEstatus)
        {
            throw new InvalidOperationException(
                "Ya existe un antecedente activo idéntico.");
        }

        antecedente.AnEstatus = true;

        await _repo.GuardarCambiosAsync();

        await AuditarAsync(
            antecedente, historia.PaId, "REACTIVAR");

        await tx.CommitAsync();

        return true;
    }

    private async Task<Historia> ValidarHistoriaActivaAsync(
        long historiaId)
    {
        var historia = await _repo.ObtenerHistoriaAsync(
            historiaId);

        if (historia == null)
            throw new ArgumentException(
                "La historia clínica no existe.");

        if (historia.HiEstad != "ACTIVA")
            throw new InvalidOperationException(
                "La historia clínica está bloqueada o cerrada.");

        if (!await _repo.PacienteActivoAsync(historia.PaId))
            throw new InvalidOperationException(
                "El paciente de la historia está inactivo.");

        return historia;
    }

    private async Task ValidarUsuarioAsync(long? usuarioId)
    {
        if (!usuarioId.HasValue)
            return;

        if (usuarioId.Value <= 0 ||
            !await _repo.UsuarioActivoAsync(usuarioId.Value))
        {
            throw new ArgumentException(
                "El usuario no existe o está inactivo.");
        }
    }

    private static void ValidarDatos(
        string tipo, string descripcion)
    {
        if (string.IsNullOrWhiteSpace(tipo) ||
            tipo.Trim().Length > 80)
        {
            throw new ArgumentException(
                "El tipo es obligatorio, máximo 80 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(descripcion) ||
            descripcion.Trim().Length > 2000)
        {
            throw new ArgumentException(
                "La descripción es obligatoria, máximo 2000 caracteres.");
        }
    }

    private async Task AuditarAsync(
        Antecedente antecedente,
        long pacienteId,
        string accion)
    {
        await _auditoria.RegistrarAsync(
            us_id: null,
            pa_id: pacienteId,
            ev_modulo: "CLINICA",
            ev_accion: accion,
            ev_entid: "ANTECEDENTE",
            ev_entid_id: antecedente.AnId.ToString(),
            ev_resul: "CORRECTO",
            ev_direc_ip: null,
            ev_agent_usua: null,
            ev_metad_json: null
        );
    }

    private static AntecedenteRespuestaDto Mapear(Antecedente a)
    {
        return new AntecedenteRespuestaDto
        {
            Id = a.AnId,
            HistoriaId = a.HiId,
            UsuarioId = a.UsId,
            Tipo = a.AnTipo,
            Descripcion = a.AnDescri,
            FechaRegistro = a.AnFechaReg,
            Estatus = a.AnEstatus
        };
    }
}
