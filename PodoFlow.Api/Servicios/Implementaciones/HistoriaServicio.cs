
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.DTOs.Historia;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class HistoriaServicio : IHistoriaServicio
{
    private readonly IHistoriaRepositorio _repo;
    private readonly IEventoAuditoriaServicio _auditoria;
    private readonly PodoFlowContext _context;

    private static readonly string[] EstadosValidos =
    {
        "ACTIVA", "BLOQUEADA", "CERRADA"
    };

    public HistoriaServicio(
        IHistoriaRepositorio repo,
        IEventoAuditoriaServicio auditoria,
        PodoFlowContext context)
    {
        _repo = repo;
        _auditoria = auditoria;
        _context = context;
    }

    public async Task<List<HistoriaRespuestaDto>>
        ObtenerTodosAsync(
            string? estado, int pagina, int tamano)
    {
        if (pagina < 1 || tamano < 1 || tamano > 100)
            throw new ArgumentException("Paginación inválida.");

        string? filtro = null;

        if (!string.IsNullOrWhiteSpace(estado))
        {
            filtro = estado.Trim().ToUpperInvariant();

            if (!EstadosValidos.Contains(filtro))
                throw new ArgumentException("Estado inválido.");
        }

        var historias = await _repo.ObtenerTodosAsync(
            filtro, pagina, tamano);

        return historias.Select(Mapear).ToList();
    }

    public async Task<HistoriaRespuestaDto?>
        ObtenerPorIdAsync(long id)
    {
        var historia = await _repo.ObtenerPorIdAsync(id);

        return historia == null ? null : Mapear(historia);
    }

    public async Task<HistoriaRespuestaDto> CrearAsync(
        HistoriaCrearDto dto)
    {
        if (!await _repo.PacienteActivoAsync(dto.PacienteId))
            throw new ArgumentException(
                "El paciente no existe o está inactivo.");

        await ValidarUsuarioAsync(dto.UsuarioId);

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var existente = await _repo.ObtenerPorPacienteAsync(
            dto.PacienteId);

        if (existente != null)
        {
            throw new InvalidOperationException(
                "El paciente ya tiene una historia clínica. " +
                "Debe consultarse o cambiarse su estado, " +
                "no crear una segunda.");
        }

        var historia = new Historia
        {
            PaId = dto.PacienteId,
            UsId = dto.UsuarioId,
            HiFechaAper = DateTime.UtcNow,
            HiEstad = "ACTIVA"
        };

        await _repo.CrearAsync(historia);

        await AuditarAsync(historia, "CREAR");

        await tx.CommitAsync();

        return Mapear(historia);
    }

    public async Task<bool> ActualizarAsync(
        long id, HistoriaActualizarDto dto)
    {
        var version = LeerVersion(dto.Version);

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var historia = await _repo.ObtenerPorIdAsync(id);

        if (historia == null)
            return false;

        if (historia.HiEstad != "ACTIVA")
            throw new InvalidOperationException(
                "Solo se pueden actualizar historias activas.");

        if (!await _repo.PacienteActivoAsync(historia.PaId))
            throw new InvalidOperationException(
                "El paciente asociado está inactivo.");

        await ValidarUsuarioAsync(dto.UsuarioId);

        _repo.EstablecerVersionOriginal(historia, version);

        historia.UsId = dto.UsuarioId;

        // Fuerza un UPDATE para comprobar la versión,
        // incluso si el usuario responsable no cambió.
        _context.Entry(historia)
            .Property(x => x.UsId)
            .IsModified = true;

        await _repo.GuardarCambiosAsync();

        await AuditarAsync(historia, "ACTUALIZAR");

        await tx.CommitAsync();

        return true;
    }

    public async Task<bool> CambiarEstadoAsync(
        long id, HistoriaCambiarEstadoDto dto)
    {
        var nuevoEstado =
            dto.NuevoEstado.Trim().ToUpperInvariant();

        if (!EstadosValidos.Contains(nuevoEstado))
            throw new ArgumentException(
                "Estado no válido.");

        var version = LeerVersion(dto.Version);

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var historia = await _repo.ObtenerPorIdAsync(id);

        if (historia == null)
            return false;

        if (!TransicionPermitida(
            historia.HiEstad, nuevoEstado))
            throw new InvalidOperationException(
                $"No se permite pasar de {historia.HiEstad} " +
                $"a {nuevoEstado}.");

        if (nuevoEstado == "ACTIVA" &&
            !await _repo.PacienteActivoAsync(historia.PaId))
            throw new InvalidOperationException(
                "No se puede activar una historia de paciente inactivo.");

        var motivo = dto.Motivo?.Trim();

        if (nuevoEstado is "BLOQUEADA" or "CERRADA")
        {
            if (string.IsNullOrWhiteSpace(motivo) ||
                motivo.Length > 500)
                throw new ArgumentException(
                    "Debes indicar un motivo de máximo 500 caracteres.");
        }

        _repo.EstablecerVersionOriginal(historia, version);

        historia.HiEstad = nuevoEstado;

        if (nuevoEstado == "BLOQUEADA")
        {
            historia.HiFechaBloq = DateTime.UtcNow;
            historia.HiMotivBloq = motivo;
        }
        else if (nuevoEstado == "CERRADA")
        {
            historia.HiFechaBloq = DateTime.UtcNow;
            historia.HiMotivBloq = motivo;
        }
        else
        {
            historia.HiFechaBloq = null;
            historia.HiMotivBloq = null;
        }

        await _repo.GuardarCambiosAsync();

        await AuditarAsync(
            historia,
            nuevoEstado switch
            {
                "BLOQUEADA" => "BLOQUEAR",
                "CERRADA" => "CERRAR",
                _ => "REACTIVAR"
            });

        await tx.CommitAsync();

        return true;
    }

    private async Task ValidarUsuarioAsync(long? usuarioId)
    {
        if (!usuarioId.HasValue)
            return;

        if (usuarioId.Value <= 0 ||
            !await _repo.UsuarioActivoAsync(usuarioId.Value))
            throw new ArgumentException(
                "El usuario no existe o está inactivo.");
    }

    private static bool TransicionPermitida(
        string anterior, string nuevo)
    {
        return anterior switch
        {
            "ACTIVA" => nuevo is
                "BLOQUEADA" or "CERRADA",

            "BLOQUEADA" => nuevo is
                "ACTIVA" or "CERRADA",

            _ => false
        };
    }

    private static byte[] LeerVersion(string version)
    {
        try
        {
            var bytes = Convert.FromBase64String(version);

            if (bytes.Length != 8)
                throw new FormatException();

            return bytes;
        }
        catch (FormatException)
        {
            throw new ArgumentException(
                "La versión debe ser Base64 válido de 8 bytes.");
        }
    }

    private async Task AuditarAsync(
        Historia historia, string accion)
    {
        await _auditoria.RegistrarAsync(
            us_id: null,
            pa_id: historia.PaId,
            ev_modulo: "CLINICA",
            ev_accion: accion,
            ev_entid: "HISTORIA",
            ev_entid_id: historia.HiId.ToString(),
            ev_resul: "CORRECTO",
            ev_direc_ip: null,
            ev_agent_usua: null,
            ev_metad_json: null
        );
    }

    private static HistoriaRespuestaDto Mapear(Historia h)
    {
        return new HistoriaRespuestaDto
        {
            Id = h.HiId,
            PacienteId = h.PaId,
            UsuarioId = h.UsId,
            FechaApertura = h.HiFechaAper,
            Estado = h.HiEstad,
            FechaBloqueo = h.HiFechaBloq,
            MotivoBloqueo = h.HiMotivBloq,
            Version = Convert.ToBase64String(h.HiVersion)
        };
    }
}
