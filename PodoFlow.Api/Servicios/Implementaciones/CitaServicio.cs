
using System.Data;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.DTOs.Cita;
using PodoFlow.Api.DTOs.CambioCita;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class CitaServicio : ICitaServicio
{
    private readonly ICitaRepositorio _repo;
    private readonly ICambioCitaServicio _cambios;
    private readonly IEventoAuditoriaServicio _auditoria;
    private readonly PodoFlowContext _context;

    private static readonly string[] EstadosValidos =
    {
        "SOLICITADA",
        "CONFIRMADA",
        "ATENDIDA",
        "CANCELADA",
        "REPROGRAMADA",
        "NO_ASISTE"
    };

    public CitaServicio(
        ICitaRepositorio repo,
        ICambioCitaServicio cambios,
        IEventoAuditoriaServicio auditoria,
        PodoFlowContext context)
    {
        _repo = repo;
        _cambios = cambios;
        _auditoria = auditoria;
        _context = context;
    }

    public async Task<List<CitaRespuestaDto>> ObtenerTodosAsync(
        long? pacienteId,
        long? usuarioId,
        string? estado,
        int pagina,
        int tamano)
    {
        if (pagina < 1 || tamano < 1 || tamano > 100)
            throw new ArgumentException("Paginación inválida.");

        if (pacienteId.HasValue && pacienteId <= 0)
            throw new ArgumentException("Paciente inválido.");

        if (usuarioId.HasValue && usuarioId <= 0)
            throw new ArgumentException("Usuario inválido.");

        string? filtro = null;

        if (!string.IsNullOrWhiteSpace(estado))
        {
            filtro = estado.Trim().ToUpperInvariant();

            if (!EstadosValidos.Contains(filtro))
                throw new ArgumentException("Estado inválido.");
        }

        var citas = await _repo.ObtenerTodosAsync(
            pacienteId, usuarioId, filtro, pagina, tamano);

        return citas.Select(Mapear).ToList();
    }

    public async Task<CitaRespuestaDto?> ObtenerPorIdAsync(long id)
    {
        var cita = await _repo.ObtenerPorIdAsync(id);
        return cita == null ? null : Mapear(cita);
    }

    public async Task<CitaRespuestaDto> CrearAsync(
        CitaCrearDto dto)
    {
        ValidarDatos(dto);
        await ValidarRelacionesAsync(dto);

        if (dto.FechaInicio <= DateTime.UtcNow)
            throw new ArgumentException(
                "No se puede reservar una fecha pasada.");

        await using var tx = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        if (!await _repo.DisponibleAsync(
            dto.UsuarioId, dto.FechaInicio, dto.FechaFin))
            throw new InvalidOperationException(
                "El horario no está disponible.");

        var cita = new Cita
        {
            PaId = dto.PacienteId,
            SeId = dto.ServicioId,
            UsId = dto.UsuarioId,
            CiFechaIni = dto.FechaInicio,
            CiFechaFin = dto.FechaFin,
            CiEstad = "SOLICITADA",
            CiCanal = dto.Canal.Trim().ToUpperInvariant(),
            CiObser = dto.Observaciones?.Trim(),
            CiFechaCrea = DateTime.UtcNow
        };

        await _repo.CrearAsync(cita);

        // Registrar el estado inicial en el historial.
        await _cambios.CrearAsync(new CambioCitaCrearDto
        {
            CitaId = cita.CiId,
            UsuarioId = null,
            EstadoAnterior = null,
            EstadoNuevo = "SOLICITADA",
            FechaAnterior = null,
            FechaNueva = cita.CiFechaIni,
            Motivo = "Creación de la cita"
        });

        await AuditarAsync(cita, "CREAR");

        await tx.CommitAsync();

        return Mapear(cita);
    }

    public async Task<bool> ActualizarAsync(
        long id, CitaActualizarDto dto)
    {
        ValidarDatos(dto);

        await using var tx = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        var cita = await _repo.ObtenerPorIdAsync(id);

        if (cita == null)
            return false;

        if (EsTerminal(cita.CiEstad))
            throw new InvalidOperationException(
                "No se puede actualizar una cita finalizada.");

        var version = LeerVersion(dto.Version);
        _repo.EstablecerVersionOriginal(cita, version);

        await ValidarRelacionesAsync(dto);

        bool cambiaHorario =
            cita.CiFechaIni != dto.FechaInicio ||
            cita.CiFechaFin != dto.FechaFin ||
            cita.UsId != dto.UsuarioId;

        if (cambiaHorario)
        {
            if (dto.FechaInicio <= DateTime.UtcNow)
                throw new ArgumentException(
                    "La nueva fecha debe ser futura.");

            if (string.IsNullOrWhiteSpace(dto.MotivoCambio))
                throw new ArgumentException(
                    "Debes indicar el motivo de reprogramación.");

            if (dto.MotivoCambio.Trim().Length > 500)
                throw new ArgumentException(
                    "El motivo supera 500 caracteres.");

            if (!await _repo.DisponibleAsync(
                dto.UsuarioId,
                dto.FechaInicio,
                dto.FechaFin,
                id))
                throw new InvalidOperationException(
                    "La nueva fecha no está disponible.");
        }

        var estadoAnterior = cita.CiEstad;
        var fechaAnterior = cita.CiFechaIni;

        cita.PaId = dto.PacienteId;
        cita.SeId = dto.ServicioId;
        cita.UsId = dto.UsuarioId;
        cita.CiFechaIni = dto.FechaInicio;
        cita.CiFechaFin = dto.FechaFin;
        cita.CiCanal = dto.Canal.Trim().ToUpperInvariant();
        cita.CiObser = dto.Observaciones?.Trim();
        cita.CiFechaModi = DateTime.UtcNow;

        if (cambiaHorario)
            cita.CiEstad = "REPROGRAMADA";

        await _repo.GuardarCambiosAsync();

        if (cambiaHorario)
        {
            await _cambios.CrearAsync(new CambioCitaCrearDto
            {
                CitaId = cita.CiId,
                UsuarioId = null,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = "REPROGRAMADA",
                FechaAnterior = fechaAnterior,
                FechaNueva = cita.CiFechaIni,
                Motivo = dto.MotivoCambio!.Trim()
            });
        }

        await AuditarAsync(cita,
            cambiaHorario ? "REPROGRAMAR" : "ACTUALIZAR");

        await tx.CommitAsync();
        return true;
    }

    public async Task<bool> CambiarEstadoAsync(
        long id, CitaCambiarEstadoDto dto)
    {
        var nuevoEstado = dto.NuevoEstado.Trim().ToUpperInvariant();

        if (!EstadosValidos.Contains(nuevoEstado))
            throw new ArgumentException("Estado no permitido.");

        if (string.IsNullOrWhiteSpace(dto.Motivo) ||
            dto.Motivo.Trim().Length > 500)
            throw new ArgumentException(
                "El motivo es obligatorio, máximo 500 caracteres.");

        await using var tx = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        var cita = await _repo.ObtenerPorIdAsync(id);
        if (cita == null)
            return false;

        var version = LeerVersion(dto.Version);
        _repo.EstablecerVersionOriginal(cita, version);

        if (!TransicionPermitida(cita.CiEstad, nuevoEstado))
            throw new InvalidOperationException(
                $"No se permite cambiar de {cita.CiEstad} " +
                $"a {nuevoEstado}.");

        var estadoAnterior = cita.CiEstad;

        cita.CiEstad = nuevoEstado;
        cita.CiFechaModi = DateTime.UtcNow;

        await _repo.GuardarCambiosAsync();

        await _cambios.CrearAsync(new CambioCitaCrearDto
        {
            CitaId = cita.CiId,
            UsuarioId = null,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = nuevoEstado,
            FechaAnterior = cita.CiFechaIni,
            FechaNueva = cita.CiFechaIni,
            Motivo = dto.Motivo.Trim()
        });

        await AuditarAsync(cita,
            nuevoEstado == "CANCELADA"
                ? "CANCELAR"
                : "CAMBIAR_ESTADO");

        await tx.CommitAsync();
        return true;
    }

    private async Task ValidarRelacionesAsync(CitaCrearDto dto)
    {
        if (!await _repo.PacienteActivoAsync(dto.PacienteId))
            throw new ArgumentException(
                "El paciente no existe o está inactivo.");

        if (!await _repo.ServicioActivoAsync(dto.ServicioId))
            throw new ArgumentException(
                "El servicio no existe o está inactivo.");

        if (dto.UsuarioId.HasValue &&
            !await _repo.UsuarioActivoAsync(dto.UsuarioId.Value))
            throw new ArgumentException(
                "El profesional no existe o está inactivo.");
    }

    private static void ValidarDatos(CitaCrearDto dto)
    {
        if (dto.PacienteId <= 0 || dto.ServicioId <= 0)
            throw new ArgumentException(
                "Paciente y servicio son obligatorios.");

        if (dto.UsuarioId.HasValue && dto.UsuarioId <= 0)
            throw new ArgumentException("Usuario inválido.");

        if (dto.FechaInicio.Kind != DateTimeKind.Utc ||
            dto.FechaFin.Kind != DateTimeKind.Utc)
            throw new ArgumentException(
                "Las fechas deben enviarse en UTC con Z.");

        if (dto.FechaFin <= dto.FechaInicio)
            throw new ArgumentException(
                "La fecha final debe ser posterior a la inicial.");

        if (dto.FechaInicio.Second != 0 ||
            dto.FechaFin.Second != 0 ||
            dto.FechaInicio.Millisecond != 0 ||
            dto.FechaFin.Millisecond != 0)
            throw new ArgumentException(
                "Utiliza horas con precisión de minutos.");

        if (string.IsNullOrWhiteSpace(dto.Canal) ||
            dto.Canal.Trim().Length > 30)
            throw new ArgumentException("Canal inválido.");

        if (dto.Observaciones?.Length > 1000)
            throw new ArgumentException(
                "Las observaciones superan 1000 caracteres.");
    }

    private static bool EsTerminal(string estado)
    {
        return estado is "ATENDIDA" or "CANCELADA" or "NO_ASISTE";
    }

    private static bool TransicionPermitida(
        string anterior, string nuevo)
    {
        return anterior switch
        {
            "SOLICITADA" => nuevo is
                "CONFIRMADA" or "CANCELADA" or
                "REPROGRAMADA" or "NO_ASISTE",

            "CONFIRMADA" => nuevo is
                "ATENDIDA" or "CANCELADA" or
                "REPROGRAMADA" or "NO_ASISTE",

            "REPROGRAMADA" => nuevo is
                "CONFIRMADA" or "CANCELADA" or "NO_ASISTE",

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
                "La versión enviada no es válida.");
        }
    }

    private async Task AuditarAsync(Cita cita, string accion)
    {
        await _auditoria.RegistrarAsync(
            us_id: null,
            pa_id: cita.PaId,
            ev_modulo: "AGENDA",
            ev_accion: accion,
            ev_entid: "CITA",
            ev_entid_id: cita.CiId.ToString(),
            ev_resul: "CORRECTO",
            ev_direc_ip: null,
            ev_agent_usua: null,
            ev_metad_json: null
        );
    }

    private static CitaRespuestaDto Mapear(Cita c)
    {
        return new CitaRespuestaDto
        {
            Id = c.CiId,
            PacienteId = c.PaId,
            ServicioId = c.SeId,
            UsuarioId = c.UsId,
            FechaInicio = c.CiFechaIni,
            FechaFin = c.CiFechaFin,
            Estado = c.CiEstad,
            Canal = c.CiCanal,
            Observaciones = c.CiObser,
            FechaCreacion = c.CiFechaCrea,
            FechaModificacion = c.CiFechaModi,
            Version = Convert.ToBase64String(c.CiVersion)
        };
    }
}
