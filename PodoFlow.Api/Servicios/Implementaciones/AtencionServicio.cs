
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.DTOs.Atencion;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class AtencionServicio : IAtencionServicio
{
    private readonly IAtencionRepositorio _repo;
    private readonly IEventoAuditoriaServicio _auditoria;
    private readonly PodoFlowContext _context;

    public AtencionServicio(
        IAtencionRepositorio repo,
        IEventoAuditoriaServicio auditoria,
        PodoFlowContext context)
    {
        _repo = repo;
        _auditoria = auditoria;
        _context = context;
    }

    public async Task<List<AtencionRespuestaDto>> ObtenerTodosAsync(
        long? historiaId, string? estado,
        int pagina, int tamano)
    {
        if (pagina < 1 || tamano < 1 || tamano > 100)
            throw new ArgumentException("Paginación inválida.");

        if (historiaId.HasValue && historiaId.Value <= 0)
            throw new ArgumentException("Historia inválida.");

        string? filtro = null;

        if (!string.IsNullOrWhiteSpace(estado))
        {
            filtro = estado.Trim().ToUpperInvariant();

            if (filtro is not
                ("ABIERTA" or "CERRADA" or "BLOQUEADA"))
                throw new ArgumentException("Estado inválido.");
        }

        var datos = await _repo.ObtenerTodosAsync(
            historiaId, filtro, pagina, tamano);

        return datos.Select(Mapear).ToList();
    }

    public async Task<AtencionRespuestaDto?> ObtenerPorIdAsync(
        long id)
    {
        var atencion = await _repo.ObtenerPorIdAsync(id);
        return atencion == null ? null : Mapear(atencion);
    }

    public async Task<AtencionRespuestaDto> CrearAsync(
        AtencionCrearDto dto)
    {
        if (dto.HistoriaId <= 0 || dto.UsuarioId <= 0)
            throw new ArgumentException(
                "Historia y usuario son obligatorios.");

        if (dto.CitaId.HasValue && dto.CitaId.Value <= 0)
            throw new ArgumentException("Cita inválida.");

        ValidarTextos(
            dto.MotivoConsulta, dto.ValoracionClinica,
            dto.Diagnostico, dto.Tratamiento, dto.Observaciones);

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var historia = await ValidarHistoriaAsync(dto.HistoriaId);

        if (!await _repo.UsuarioActivoAsync(dto.UsuarioId))
            throw new ArgumentException(
                "El usuario responsable no existe o está inactivo.");

        if (dto.CitaId.HasValue)
        {
            var cita = await _repo.ObtenerCitaAsync(
                dto.CitaId.Value);

            if (cita == null)
                throw new ArgumentException("La cita no existe.");

            if (cita.PaId != historia.PaId)
                throw new InvalidOperationException(
                    "La cita pertenece a otro paciente.");

            if (cita.CiEstad != "ATENDIDA")
                throw new InvalidOperationException(
                    "La cita debe estar ATENDIDA para vincularla.");

            if (await _repo.CitaAsociadaAsync(dto.CitaId.Value))
                throw new InvalidOperationException(
                    "La cita ya tiene una atención registrada.");
        }

        var atencion = new Atencion
        {
            HiId = dto.HistoriaId,
            CiId = dto.CitaId,
            UsId = dto.UsuarioId,
            AtFecha = DateTime.UtcNow,
            AtMotivCons = Limpiar(dto.MotivoConsulta),
            AtValorClin = Limpiar(dto.ValoracionClinica),
            AtDiagn = Limpiar(dto.Diagnostico),
            AtTrata = Limpiar(dto.Tratamiento),
            AtObser = Limpiar(dto.Observaciones),
            AtEstad = "ABIERTA"
        };

        await _repo.CrearAsync(atencion);
        await AuditarAsync(atencion, historia.PaId, "CREAR");

        await tx.CommitAsync();
        return Mapear(atencion);
    }

    public async Task<bool> ActualizarAsync(
        long id, AtencionActualizarDto dto)
    {
        ValidarTextos(
            dto.MotivoConsulta, dto.ValoracionClinica,
            dto.Diagnostico, dto.Tratamiento, dto.Observaciones);

        var version = LeerVersion(dto.Version);

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var atencion = await _repo.ObtenerPorIdAsync(id);
        if (atencion == null) return false;

        if (atencion.AtEstad != "ABIERTA")
            throw new InvalidOperationException(
                "Solo pueden editarse atenciones abiertas.");

        var historia = await ValidarHistoriaAsync(atencion.HiId);

        _repo.EstablecerVersionOriginal(atencion, version);

        atencion.AtMotivCons = Limpiar(dto.MotivoConsulta);
        atencion.AtValorClin = Limpiar(dto.ValoracionClinica);
        atencion.AtDiagn = Limpiar(dto.Diagnostico);
        atencion.AtTrata = Limpiar(dto.Tratamiento);
        atencion.AtObser = Limpiar(dto.Observaciones);
        atencion.AtFechaModi = DateTime.UtcNow;

        // Obliga a emitir UPDATE y comprobar ROWVERSION.
        _context.Entry(atencion)
            .Property(x => x.AtFechaModi)
            .IsModified = true;

        await _repo.GuardarCambiosAsync();
        await AuditarAsync(atencion, historia.PaId, "ACTUALIZAR");

        await tx.CommitAsync();
        return true;
    }

    public async Task<bool> CambiarEstadoAsync(
        long id, AtencionCambiarEstadoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NuevoEstado))
            throw new ArgumentException("Estado obligatorio.");

        var nuevo = dto.NuevoEstado.Trim().ToUpperInvariant();

        if (nuevo is not ("CERRADA" or "BLOQUEADA"))
            throw new ArgumentException(
                "Solo se permite CERRADA o BLOQUEADA.");

        if (string.IsNullOrWhiteSpace(dto.Motivo) ||
            dto.Motivo.Trim().Length > 500)
            throw new ArgumentException(
                "El motivo es obligatorio (máximo 500 caracteres).");

        var version = LeerVersion(dto.Version);

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var atencion = await _repo.ObtenerPorIdAsync(id);
        if (atencion == null) return false;

        if (atencion.AtEstad != "ABIERTA")
            throw new InvalidOperationException(
                "La atención ya está cerrada o bloqueada.");

        var historia = await ValidarHistoriaAsync(atencion.HiId);

        _repo.EstablecerVersionOriginal(atencion, version);

        atencion.AtEstad = nuevo;
        atencion.AtFechaModi = DateTime.UtcNow;

        await _repo.GuardarCambiosAsync();

        // El motivo se registra sin incluir contenido clínico
        // en los metadatos de auditoría.
        await AuditarAsync(
            atencion, historia.PaId,
            nuevo == "CERRADA" ? "CERRAR" : "BLOQUEAR");

        await tx.CommitAsync();
        return true;
    }

    private async Task<Historia> ValidarHistoriaAsync(
        long historiaId)
    {
        var historia = await _repo.ObtenerHistoriaAsync(historiaId);

        if (historia == null)
            throw new ArgumentException(
                "La historia clínica no existe.");

        if (historia.HiEstad != "ACTIVA")
            throw new InvalidOperationException(
                "La historia está bloqueada o cerrada.");

        if (!await _repo.PacienteActivoAsync(historia.PaId))
            throw new InvalidOperationException(
                "El paciente está inactivo.");

        return historia;
    }

    private static string? Limpiar(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor)
            ? null : valor.Trim();
    }

    private static void ValidarTextos(
        string? motivo, string? valoracion,
        string? diagnostico, string? tratamiento,
        string? observaciones)
    {
        if (motivo?.Length > 1000 ||
            valoracion?.Length > 3000 ||
            diagnostico?.Length > 2000 ||
            tratamiento?.Length > 3000 ||
            observaciones?.Length > 3000)
            throw new ArgumentException(
                "Uno de los campos supera su longitud permitida.");
    }

    private static byte[] LeerVersion(string? valor)
    {
        try
        {
            var bytes = Convert.FromBase64String(valor ?? "");

            if (bytes.Length != 8)
                throw new FormatException();

            return bytes;
        }
        catch (FormatException)
        {
            throw new ArgumentException(
                "Versión inválida. Utiliza la versión del GET.");
        }
    }

    private async Task AuditarAsync(
        Atencion atencion, long pacienteId, string accion)
    {
        await _auditoria.RegistrarAsync(
            us_id: null,
            pa_id: pacienteId,
            ev_modulo: "CLINICA",
            ev_accion: accion,
            ev_entid: "ATENCION",
            ev_entid_id: atencion.AtId.ToString(),
            ev_resul: "CORRECTO",
            ev_direc_ip: null,
            ev_agent_usua: null,
            ev_metad_json: null);
    }

    private static AtencionRespuestaDto Mapear(Atencion a)
    {
        return new AtencionRespuestaDto
        {
            Id = a.AtId,
            HistoriaId = a.HiId,
            CitaId = a.CiId,
            UsuarioId = a.UsId,
            Fecha = a.AtFecha,
            MotivoConsulta = a.AtMotivCons,
            ValoracionClinica = a.AtValorClin,
            Diagnostico = a.AtDiagn,
            Tratamiento = a.AtTrata,
            Observaciones = a.AtObser,
            Estado = a.AtEstad,
            FechaModificacion = a.AtFechaModi,
            Version = Convert.ToBase64String(a.AtVersion)
        };
    }
}
