
using System.Text.Json;
using PodoFlow.Api.DTOs.EventoAuditoria;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class EventoAuditoriaServicio
    : IEventoAuditoriaServicio
{
    private readonly IEventoAuditoriaRepositorio _repositorio;

    private static readonly string[] ResultadosValidos =
    {
        "CORRECTO", "ERROR", "DENEGADO"
    };

    public EventoAuditoriaServicio(
        IEventoAuditoriaRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<EventoAuditoriaRespuestaDto> RegistrarAsync(
        long? us_id,
        long? pa_id,
        string ev_modulo,
        string ev_accion,
        string? ev_entid,
        string? ev_entid_id,
        string ev_resul,
        string? ev_direc_ip,
        string? ev_agent_usua,
        string? ev_metad_json)
    {
        if (us_id.HasValue && us_id <= 0)
            throw new ArgumentException("ID de usuario inválido.");

        if (pa_id.HasValue && pa_id <= 0)
            throw new ArgumentException("ID de paciente inválido.");

        if (string.IsNullOrWhiteSpace(ev_modulo) ||
            string.IsNullOrWhiteSpace(ev_accion))
            throw new ArgumentException(
                "El módulo y la acción son obligatorios.");

        var modulo = ev_modulo.Trim().ToUpperInvariant();
        var accion = ev_accion.Trim().ToUpperInvariant();
        var resultado = ev_resul.Trim().ToUpperInvariant();

        if (!ResultadosValidos.Contains(resultado))
            throw new ArgumentException(
                "Resultado válido: CORRECTO, ERROR o DENEGADO.");

        if (modulo.Length > 80 || accion.Length > 80)
            throw new ArgumentException(
                "El módulo o la acción supera 80 caracteres.");

        ValidarLongitud(ev_entid, 128, "Entidad");
        ValidarLongitud(ev_entid_id, 100, "EntidadId");
        ValidarLongitud(ev_direc_ip, 45, "Dirección IP");
        ValidarLongitud(ev_agent_usua, 500, "Agente de usuario");

        if (!string.IsNullOrWhiteSpace(ev_metad_json))
        {
            try
            {
                using var documento =
                    JsonDocument.Parse(ev_metad_json);

                if (documento.RootElement.ValueKind
                    != JsonValueKind.Object)
                    throw new ArgumentException(
                        "Los metadatos deben ser un objeto JSON.");
            }
            catch (JsonException)
            {
                throw new ArgumentException(
                    "Los metadatos no contienen un JSON válido.");
            }
        }

        var evento = new EventoAuditoria
        {
            UsId = us_id,
            PaId = pa_id,
            EvFechaHora = DateTime.UtcNow,
            EvModulo = modulo,
            EvAccion = accion,
            EvEntid = ev_entid?.Trim(),
            EvEntidId = ev_entid_id?.Trim(),
            EvResul = resultado,
            EvDirecIp = ev_direc_ip?.Trim(),
            EvAgentUsua = ev_agent_usua?.Trim(),
            EvIdCorre = Guid.NewGuid(),
            EvMetadJson = string.IsNullOrWhiteSpace(ev_metad_json)
                ? null
                : ev_metad_json
        };

        var creado = await _repositorio.CrearAsync(evento);

        return MapearRespuesta(creado);
    }

    public async Task<EventoAuditoriaRespuestaDto?>
        ObtenerPorIdAsync(long id)
    {
        var evento = await _repositorio.ObtenerPorIdAsync(id);

        return evento == null
            ? null
            : MapearRespuesta(evento);
    }

    public async Task<List<EventoAuditoriaRespuestaDto>>
        ObtenerTodosAsync(
            long? usuarioId,
            long? pacienteId,
            string? modulo,
            string? resultado,
            DateTime? desde,
            DateTime? hasta,
            int pagina,
            int tamano)
    {
        if (desde.HasValue && hasta.HasValue &&
            desde.Value > hasta.Value)
            throw new ArgumentException(
                "La fecha inicial no puede superar la final.");

        if (pagina < 1 || tamano < 1 || tamano > 100)
            throw new ArgumentException("Paginación inválida.");

        if (usuarioId.HasValue && usuarioId <= 0)
            throw new ArgumentException("Usuario inválido.");

        if (pacienteId.HasValue && pacienteId <= 0)
            throw new ArgumentException("Paciente inválido.");

        var filtroResultado = resultado?.Trim().ToUpperInvariant();

        if (!string.IsNullOrEmpty(filtroResultado) &&
            !ResultadosValidos.Contains(filtroResultado))
            throw new ArgumentException("Resultado inválido.");

        var filtroModulo = modulo?.Trim().ToUpperInvariant();

        if (filtroModulo?.Length > 80)
            throw new ArgumentException("Módulo demasiado largo.");

        var eventos = await _repositorio.ObtenerTodosAsync(
            usuarioId,
            pacienteId,
            filtroModulo,
            filtroResultado,
            desde,
            hasta,
            pagina,
            tamano);

        return eventos.Select(MapearRespuesta).ToList();
    }

    private static void ValidarLongitud(
        string? valor, int maximo, string campo)
    {
        if (valor != null && valor.Length > maximo)
            throw new ArgumentException(
                $"{campo} no puede superar {maximo} caracteres.");
    }

    private static EventoAuditoriaRespuestaDto MapearRespuesta(
        EventoAuditoria evento)
    {
        return new EventoAuditoriaRespuestaDto
        {
            Id = evento.EvId,
            UsuarioId = evento.UsId,
            PacienteId = evento.PaId,
            FechaHora = evento.EvFechaHora,
            Modulo = evento.EvModulo,
            Accion = evento.EvAccion,
            Entidad = evento.EvEntid,
            EntidadId = evento.EvEntidId,
            Resultado = evento.EvResul,
            DireccionIp = evento.EvDirecIp,
            AgenteUsuario = evento.EvAgentUsua,
            IdCorrelacion = evento.EvIdCorre,
            MetadatosJson = evento.EvMetadJson
        };
    }
}
