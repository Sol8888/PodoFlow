
using PodoFlow.Api.Datos;
using PodoFlow.Api.DTOs.Adjunto;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class AdjuntoServicio : IAdjuntoServicio
{
    private readonly IAdjuntoRepositorio _repo;
    private readonly IEventoAuditoriaServicio _auditoria;
    private readonly PodoFlowContext _context;

    public AdjuntoServicio(
        IAdjuntoRepositorio repo,
        IEventoAuditoriaServicio auditoria,
        PodoFlowContext context)
    {
        _repo = repo;
        _auditoria = auditoria;
        _context = context;
    }

    public async Task<List<AdjuntoRespuestaDto>> ObtenerTodosAsync(
        long? atencionId,
        bool incluirInactivos,
        int pagina,
        int tamano)
    {
        if (pagina < 1 || tamano < 1 || tamano > 100)
            throw new ArgumentException("Paginación inválida.");

        if (atencionId.HasValue && atencionId.Value <= 0)
            throw new ArgumentException("Atención inválida.");

        var adjuntos = await _repo.ObtenerTodosAsync(
            atencionId, incluirInactivos, pagina, tamano);

        return adjuntos.Select(Mapear).ToList();
    }

    public async Task<AdjuntoRespuestaDto?> ObtenerPorIdAsync(long id)
    {
        var adjunto = await _repo.ObtenerPorIdAsync(id);

        return adjunto == null ? null : Mapear(adjunto);
    }

    public async Task<AdjuntoRespuestaDto> CrearAsync(
        AdjuntoCrearDto dto)
    {
        ValidarDatos(
            dto.Nombre, dto.RutaArchivo, dto.TipoMime,
            dto.Huella, dto.TamanoBytes);

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var (_, historia) =
            await ValidarAtencionAbiertaAsync(dto.AtencionId);

        if (dto.UsuarioId <= 0 ||
            !await _repo.UsuarioActivoAsync(dto.UsuarioId))
            throw new ArgumentException(
                "El usuario no existe o está inactivo.");

        var ruta = dto.RutaArchivo.Trim();

        var existente = await _repo.ObtenerPorRutaAsync(ruta);

        if (existente != null)
        {
            // Una ruta registrada pertenece siempre
            // al mismo adjunto, atención y responsable.
            if (existente.AtId != dto.AtencionId ||
                existente.UsId != dto.UsuarioId)
                throw new InvalidOperationException(
                    "La ruta ya está asociada a otro registro.");

            if (existente.AdEstatus)
                throw new InvalidOperationException(
                    "Ya existe un adjunto activo con esa ruta.");

            // No recuperar contenido distinto bajo una misma ruta.
            if (existente.AdHuella != NormalizarHuella(dto.Huella) ||
                existente.AdTamanByte != dto.TamanoBytes)
                throw new InvalidOperationException(
                    "Los metadatos no coinciden con el adjunto anterior.");

            existente.AdEstatus = true;

            await _repo.GuardarCambiosAsync();
            await AuditarAsync(
                existente, historia.PaId, "REACTIVAR");

            await tx.CommitAsync();
            return Mapear(existente);
        }

        var adjunto = new Adjunto
        {
            AtId = dto.AtencionId,
            UsId = dto.UsuarioId,
            AdNombre = dto.Nombre.Trim(),
            AdRutaArch = ruta,
            AdTipoMime = Limpiar(dto.TipoMime),
            AdHuella = NormalizarHuella(dto.Huella),
            AdTamanByte = dto.TamanoBytes,
            AdFechaCarg = DateTime.UtcNow,
            AdEstatus = true
        };

        var creado = await _repo.CrearAsync(adjunto);

        await AuditarAsync(creado, historia.PaId, "CREAR");

        await tx.CommitAsync();
        return Mapear(creado);
    }

    public async Task<bool> ActualizarAsync(
        long id, AdjuntoActualizarDto dto)
    {
        ValidarDatos(
            dto.Nombre, dto.RutaArchivo, dto.TipoMime,
            dto.Huella, dto.TamanoBytes);

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var adjunto = await _repo.ObtenerPorIdAsync(id);

        if (adjunto == null)
            return false;

        if (!adjunto.AdEstatus)
            throw new InvalidOperationException(
                "No se puede actualizar un adjunto inactivo.");

        var (_, historia) =
            await ValidarAtencionAbiertaAsync(adjunto.AtId);

        var ruta = dto.RutaArchivo.Trim();

        // Para evitar sustituir archivos clínicos
        // silenciosamente, la ruta es inmutable.
        if (ruta != adjunto.AdRutaArch)
            throw new InvalidOperationException(
                "No se permite cambiar la ruta del archivo. " +
                "Registra un adjunto nuevo para otro archivo.");

        if (adjunto.AdHuella != NormalizarHuella(dto.Huella) ||
            adjunto.AdTamanByte != dto.TamanoBytes ||
            adjunto.AdTipoMime != Limpiar(dto.TipoMime))
            throw new InvalidOperationException(
                "No se permite cambiar la huella, tamaño " +
                "ni tipo MIME de un archivo ya registrado.");

        adjunto.AdNombre = dto.Nombre.Trim();

        await _repo.GuardarCambiosAsync();

        await AuditarAsync(
            adjunto, historia.PaId, "ACTUALIZAR");

        await tx.CommitAsync();

        return true;
    }

    public async Task<bool> EliminarAsync(long id)
    {
        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var adjunto = await _repo.ObtenerPorIdAsync(id);

        if (adjunto == null)
            return false;

        if (!adjunto.AdEstatus)
            return true;

        var (_, historia) =
            await ValidarAtencionAbiertaAsync(adjunto.AtId);

        adjunto.AdEstatus = false;

        await _repo.GuardarCambiosAsync();

        await AuditarAsync(
            adjunto, historia.PaId, "BAJA_LOGICA");

        await tx.CommitAsync();

        return true;
    }

    public async Task<bool> ReactivarAsync(long id)
    {
        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var adjunto = await _repo.ObtenerPorIdAsync(id);

        if (adjunto == null)
            return false;

        if (adjunto.AdEstatus)
            return true;

        var (_, historia) =
            await ValidarAtencionAbiertaAsync(adjunto.AtId);

        if (!await _repo.UsuarioActivoAsync(adjunto.UsId))
            throw new InvalidOperationException(
                "El usuario responsable está inactivo.");

        adjunto.AdEstatus = true;

        await _repo.GuardarCambiosAsync();

        await AuditarAsync(
            adjunto, historia.PaId, "REACTIVAR");

        await tx.CommitAsync();

        return true;
    }

    private async Task<(Atencion, Historia)>
        ValidarAtencionAbiertaAsync(long atencionId)
    {
        var atencion = await _repo.ObtenerAtencionAsync(atencionId);

        if (atencion == null)
            throw new ArgumentException("La atención no existe.");

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

    private static void ValidarDatos(
        string? nombre,
        string? ruta,
        string? tipoMime,
        string? huella,
        long? tamano)
    {
        if (string.IsNullOrWhiteSpace(nombre) ||
            nombre.Trim().Length > 255)
            throw new ArgumentException(
                "Nombre obligatorio, máximo 255 caracteres.");

        if (string.IsNullOrWhiteSpace(ruta) ||
            ruta.Trim().Length > 1000)
            throw new ArgumentException(
                "Ruta obligatoria, máximo 1000 caracteres.");

        // Clave interna, no URL externa ni ruta del sistema.
        if (!ruta.StartsWith("adjuntos/", StringComparison.Ordinal) ||
            ruta.Contains("..") ||
            ruta.Contains('\\') ||
            ruta.Contains('?') ||
            ruta.Contains('#') ||
            ruta.Contains("://"))
            throw new ArgumentException(
                "La ruta debe ser una clave interna " +
                "con prefijo adjuntos/.");

        if (tipoMime?.Length > 150)
            throw new ArgumentException("Tipo MIME demasiado largo.");

        if (huella != null && huella.Length > 128)
            throw new ArgumentException("Huella demasiado larga.");

        if (tamano.HasValue && tamano.Value < 0)
            throw new ArgumentException(
                "El tamaño no puede ser negativo.");
    }

    private static string? Limpiar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static string? NormalizarHuella(string? huella) =>
        Limpiar(huella)?.ToLowerInvariant();

    private async Task AuditarAsync(
        Adjunto adjunto, long pacienteId, string accion)
    {
        await _auditoria.RegistrarAsync(
            us_id: null,
            pa_id: pacienteId,
            ev_modulo: "CLINICA",
            ev_accion: accion,
            ev_entid: "ADJUNTO",
            ev_entid_id: adjunto.AdId.ToString(),
            ev_resul: "CORRECTO",
            ev_direc_ip: null,
            ev_agent_usua: null,
            ev_metad_json: null
        );
    }

    private static AdjuntoRespuestaDto Mapear(Adjunto a)
    {
        return new AdjuntoRespuestaDto
        {
            Id = a.AdId,
            AtencionId = a.AtId,
            UsuarioId = a.UsId,
            Nombre = a.AdNombre,
            RutaArchivo = a.AdRutaArch,
            TipoMime = a.AdTipoMime,
            Huella = a.AdHuella,
            TamanoBytes = a.AdTamanByte,
            FechaCarga = a.AdFechaCarg,
            Estatus = a.AdEstatus
        };
    }
}
