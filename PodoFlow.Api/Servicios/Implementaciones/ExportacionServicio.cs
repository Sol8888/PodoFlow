using PodoFlow.Api.DTOs.Exportacion;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    public class ExportacionServicio : IExportacionServicio
    {
        private const string Generada = "GENERADA";
        private const string Entregada = "ENTREGADA";
        private const string Anulada = "ANULADA";
        private static readonly HashSet<string> Formatos = ["JSON", "CSV", "PDF", "ZIP"];
        private static readonly HashSet<string> FormatosPortabilidad = ["JSON", "CSV", "ZIP"];
        private static readonly HashSet<string> DerechosConExportacion = ["ACCESO", "PORTABILIDAD"];

        private static readonly HashSet<string> EstadosSolicitudPermitidos = ["EN_PROCESO", "RESPONDIDA"];
        private static readonly Dictionary<string, string[]> TransicionesPermitidas = new()
        {
            [Generada] = [Entregada, Anulada],
            [Entregada] = [],
            [Anulada] = []
        };

        private readonly IExportacionRepositorio _repositorio;
        private readonly ISolicitudRepositorio _solicitudRepositorio;

        public ExportacionServicio(
            IExportacionRepositorio repositorio,
            ISolicitudRepositorio solicitudRepositorio)
        {
            _repositorio = repositorio;
            _solicitudRepositorio = solicitudRepositorio;
        }

        public async Task<List<ExportacionRespuestaDto>> ObtenerTodosAsync(int pagina, int tamano)
        {
            var exportaciones = await _repositorio.ObtenerTodosAsync(pagina, tamano);

            return exportaciones.Select(MapearRespuesta).ToList();
        }
        public async Task<List<ExportacionRespuestaDto>?> ObtenerPorSolicitudAsync(long solicitudId)
        {
            var solicitud = await _solicitudRepositorio.ObtenerPorIdAsync(solicitudId);

            if (solicitud == null)
                return null;

            var exportaciones = await _repositorio.ObtenerPorSolicitudAsync(solicitudId);

            return exportaciones.Select(MapearRespuesta).ToList();
        }

        public async Task<ExportacionRespuestaDto?> ObtenerPorIdAsync(long id)
        {
            var exportacion = await _repositorio.ObtenerPorIdAsync(id);

            return exportacion == null ? null : MapearRespuesta(exportacion);
        }

        public async Task<ExportacionRespuestaDto> CrearAsync(
            ExportacionCrearDto dto)
        {
            var formato = Normalizar(dto.Formato);

            if (!Formatos.Contains(formato))
                throw new ArgumentException("El formato debe ser JSON, CSV, PDF o ZIP.");

            var solicitud = await _solicitudRepositorio.ObtenerPorIdAsync(dto.SolicitudId);

            if (solicitud == null)
                throw new ArgumentException($"No existe la solicitud {dto.SolicitudId}.");

            if (!DerechosConExportacion.Contains(solicitud.SoTipoDere))
                throw new InvalidOperationException(
                    $"Una solicitud de {solicitud.SoTipoDere} no genera exportación de datos. Solo ACCESO o PORTABILIDAD.");

            if (!EstadosSolicitudPermitidos.Contains(solicitud.SoEstad))
                throw new InvalidOperationException(
                    $"La solicitud está {solicitud.SoEstad}. Solo se exporta cuando está EN_PROCESO o RESPONDIDA.");

            if (solicitud.SoTipoDere == "PORTABILIDAD" && !FormatosPortabilidad.Contains(formato))
                throw new ArgumentException(
                    "Para PORTABILIDAD el formato debe ser estructurado: JSON, CSV o ZIP.");

            await ValidarUsuarioAsync(dto.UsuarioId);

            var ahora = DateTime.UtcNow;

            var exportacion = new Exportacion
            {
                SoId = dto.SolicitudId,
                UsId = dto.UsuarioId,
                ExForma = formato,
                ExRutaArch = dto.RutaArchivo.Trim(),
                ExHuella = string.IsNullOrWhiteSpace(dto.Huella) ? null : dto.Huella.Trim(),
                ExFechaGene = ahora,
                ExEstad = Generada
            };

            var accion = CrearAccion(dto.SolicitudId, dto.UsuarioId, ahora,
                $"Exportación generada en {formato}", dto.Observacion);

            var creada = await _repositorio.CrearAsync(exportacion, accion);

            return MapearRespuesta(creada);
        }

        public async Task<ExportacionRespuestaDto?> CambiarEstadoAsync(
            long id,
            ExportacionCambiarEstadoDto dto)
        {
            var nuevoEstado = Normalizar(dto.Estado);

            if (!TransicionesPermitidas.ContainsKey(nuevoEstado))
                throw new ArgumentException("El estado debe ser GENERADA, ENTREGADA o ANULADA.");

            var exportacion = await _repositorio.ObtenerPorIdAsync(id);

            if (exportacion == null)
                return null;

            var estadoAnterior = exportacion.ExEstad;

            if (!TransicionesPermitidas[estadoAnterior].Contains(nuevoEstado))
                throw new InvalidOperationException(
                    $"No se puede pasar de {estadoAnterior} a {nuevoEstado}.");

            await ValidarUsuarioAsync(dto.UsuarioId);

            var ahora = DateTime.UtcNow;

            if (nuevoEstado == Entregada)
                exportacion.ExFechaEntr = ahora;

            exportacion.ExEstad = nuevoEstado;

            var accion = CrearAccion(exportacion.SoId, dto.UsuarioId, ahora,
                $"Exportación {exportacion.ExId} {(nuevoEstado == Entregada ? "entregada" : "anulada")}",
                dto.Observacion);

            await _repositorio.CambiarEstadoAsync(accion);

            return MapearRespuesta(exportacion);
        }

        private async Task ValidarUsuarioAsync(long usuarioId)
        {
            if (!await _solicitudRepositorio.ExisteUsuarioActivoAsync(usuarioId))
                throw new ArgumentException($"No existe un usuario activo con id {usuarioId}.");
        }

        private static AccionSolicitud CrearAccion(
            long solicitudId, long usuarioId, DateTime fecha, string accion, string? observacion)
        {
            return new AccionSolicitud
            {
                SoId = solicitudId,
                UsId = usuarioId,
                AcFechaHora = fecha,
                AcEstadAnte = null,
                AcEstadNuev = null,
                AcAccion = accion,
                AcObser = observacion?.Trim()
            };
        }

        private static string Normalizar(string valor)
        {
            return valor.Trim().ToUpperInvariant();
        }

        private static ExportacionRespuestaDto MapearRespuesta(
            Exportacion exportacion)
        {
            return new ExportacionRespuestaDto
            {
                Id = exportacion.ExId,
                SolicitudId = exportacion.SoId,
                UsuarioId = exportacion.UsId,
                Formato = exportacion.ExForma,
                RutaArchivo = exportacion.ExRutaArch,
                Huella = exportacion.ExHuella,
                FechaGeneracion = exportacion.ExFechaGene,
                FechaEntrega = exportacion.ExFechaEntr,
                Estado = exportacion.ExEstad
            };
        }
    }
}