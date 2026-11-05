using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using PodoFlow.Api.DTOs.Exportacion;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;
using QuestPDF.Fluent;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    public class ExportacionServicio : IExportacionServicio
    {
        private const string Generada = "GENERADA";
        private const string Entregada = "ENTREGADA";
        private const string Anulada = "ANULADA";
        private const string Json = "JSON";
        private const string Csv = "CSV";
        private const string Pdf = "PDF";
        private const string DerechoAcceso = "ACCESO";
        private const string SolicitudRechazada = "RECHAZADA";
        private static readonly HashSet<string> Formatos = [Json, Csv, Pdf];
        private static readonly HashSet<string> DerechosConExportacion = ["ACCESO", "PORTABILIDAD"];

        private static readonly HashSet<string> EstadosSolicitudPermitidos = ["EN_PROCESO", "RESPONDIDA"];
        private static readonly Dictionary<string, string[]> TransicionesPermitidas = new()
        {
            [Generada] = [Entregada, Anulada],
            [Entregada] = [],
            [Anulada] = []
        };

        private static readonly Dictionary<string, string[]> SeccionesExportables = new()
        {
            ["PERSONALES"] = ["paciente", "contactos"],
            ["CITAS"] = ["citas"],
            ["CONSENTIMIENTOS"] = ["consentimientos", "eventosConsentimiento"],
            ["COMUNICACIONES"] = ["opcionesComunicacion", "notificaciones", "envios"],
            ["SOLICITUDES"] = ["solicitudes"],
            ["CLINICA"] = ["historia", "antecedentes", "atenciones", "evoluciones", "adjuntos"]
        };

        private static readonly JsonSerializerOptions OpcionesJson = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private readonly IExportacionRepositorio _repositorio;
        private readonly ISolicitudRepositorio _solicitudRepositorio;
        private readonly IConfiguration _configuration;

        public ExportacionServicio(
            IExportacionRepositorio repositorio,
            ISolicitudRepositorio solicitudRepositorio,
            IConfiguration configuration)
        {
            _repositorio = repositorio;
            _solicitudRepositorio = solicitudRepositorio;
            _configuration = configuration;
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

        public async Task<ExportacionRespuestaDto> CrearAsync(ExportacionCrearDto dto)
        {
            var formato = Normalizar(dto.Formato);

            if (!Formatos.Contains(formato))
                throw new ArgumentException("El formato debe ser JSON, CSV o PDF.");

            var secciones = ObtenerSecciones(dto.Secciones);

            var solicitud = await ObtenerSolicitudExportableAsync(dto.SolicitudId);

            if (formato == Pdf && solicitud.SoTipoDere != DerechoAcceso)
                throw new ArgumentException(
                    $"El PDF solo se genera para el derecho de ACCESO. Para {solicitud.SoTipoDere} use un formato estructurado: JSON o CSV.");

            await ValidarUsuarioAsync(dto.UsuarioId);

            var ahora = AhoraSinFraccion();

            var datos = new Dictionary<string, List<Dictionary<string, object?>>>();

            foreach (var seccion in secciones)
                foreach (var conjunto in SeccionesExportables[seccion])
                    datos[conjunto] = await _repositorio.ObtenerDatosTitularAsync(conjunto, solicitud.PaId);

            var metadatos = new Dictionary<string, object?>
            {
                ["solicitudId"] = solicitud.SoId,
                ["derecho"] = solicitud.SoTipoDere,
                ["formato"] = formato,
                ["fechaGeneracion"] = ahora,
                ["secciones"] = secciones
            };

            var sufijo = Guid.NewGuid().ToString("N")[..8];

            var contenido = formato switch
            {
                Json => ArmarJson(metadatos, datos),
                Csv => ArmarCsv(metadatos, datos),
                _ => ArmarPdf(solicitud.SoId, $"S{solicitud.SoId}-{ahora:yyyyMMddHHmmss}-{sufijo.ToUpperInvariant()}", ahora, secciones, datos)
            };

            var carpeta = ObtenerCarpeta();
            Directory.CreateDirectory(carpeta);

            var extension = $".{formato.ToLowerInvariant()}";
            var nombreArchivo = $"solicitud-{solicitud.SoId}_{ahora:yyyyMMddHHmmss}_{sufijo}{extension}";
            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            await using (var archivo = new FileStream(rutaCompleta, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await archivo.WriteAsync(contenido);
            }

            var exportacion = new Exportacion
            {
                SoId = solicitud.SoId,
                UsId = dto.UsuarioId,
                ExForma = formato,
                ExRutaArch = nombreArchivo,
                ExHuella = CalcularHuella(contenido),
                ExFechaGene = ahora,
                ExEstad = Generada
            };

            var accion = CrearAccion(solicitud.SoId, dto.UsuarioId, ahora,
                $"Exportación generada en {formato} con las secciones {string.Join(", ", secciones)}",
                dto.Observacion);

            try
            {
                var creada = await _repositorio.CrearAsync(exportacion, accion);

                return MapearRespuesta(creada);
            }
            catch
            {
                File.Delete(rutaCompleta);
                throw;
            }
        }

        public async Task<(byte[] Contenido, string NombreArchivo, string TipoContenido)?> DescargarAsync(long id, long usuarioId)
        {
            var exportacion = await _repositorio.ObtenerPorIdAsync(id);

            if (exportacion == null)
                return null;

            await ValidarUsuarioAsync(usuarioId);

            if (exportacion.ExEstad == Anulada)
                throw new InvalidOperationException("La exportación está ANULADA y no se puede entregar.");

            var solicitud = await _solicitudRepositorio.ObtenerPorIdAsync(exportacion.SoId);

            if (solicitud == null || solicitud.SoEstad == SolicitudRechazada)
                throw new InvalidOperationException(
                    "La solicitud fue RECHAZADA; su exportación no se puede entregar.");

            if (string.IsNullOrWhiteSpace(exportacion.ExHuella))
                throw new InvalidOperationException(
                    "La exportación no tiene huella registrada y no se puede verificar su integridad.");

            var ruta = ResolverRuta(exportacion.ExRutaArch);

            if (ruta == null)
                throw new InvalidOperationException(
                    "El archivo no está en el almacenamiento de exportaciones de PodoFlow (la exportación se registró manualmente).");

            if (!File.Exists(ruta))
                throw new InvalidOperationException("No se encontró el archivo de la exportación en el almacenamiento.");

            var contenido = await File.ReadAllBytesAsync(ruta);

            if (!string.Equals(CalcularHuella(contenido), exportacion.ExHuella.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                await _repositorio.RegistrarAccionAsync(CrearAccion(exportacion.SoId, usuarioId, AhoraSinFraccion(),
                    $"Verificación de integridad fallida en la exportación {exportacion.ExId}", null));

                throw new InvalidOperationException(
                    "El archivo no coincide con la huella registrada y pudo haber sido alterado. Anule esta exportación y genere una nueva.");
            }

            await _repositorio.RegistrarAccionAsync(CrearAccion(exportacion.SoId, usuarioId, AhoraSinFraccion(),
                $"Exportación {exportacion.ExId} descargada para entrega", null));

            return (contenido, Path.GetFileName(ruta), TipoContenido(exportacion.ExForma));
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

        private async Task<Solicitud> ObtenerSolicitudExportableAsync(long solicitudId)
        {
            var solicitud = await _solicitudRepositorio.ObtenerPorIdAsync(solicitudId);

            if (solicitud == null)
                throw new ArgumentException($"No existe la solicitud {solicitudId}.");

            if (!DerechosConExportacion.Contains(solicitud.SoTipoDere))
                throw new InvalidOperationException(
                    $"Una solicitud de {solicitud.SoTipoDere} no genera exportación de datos. Solo ACCESO o PORTABILIDAD.");

            if (!EstadosSolicitudPermitidos.Contains(solicitud.SoEstad))
                throw new InvalidOperationException(
                    $"La solicitud está {solicitud.SoEstad}. Solo se exporta cuando está EN_PROCESO o RESPONDIDA.");

            if (!await _repositorio.ExisteEvidenciaAsync(solicitudId))
                throw new InvalidOperationException(
                    "La solicitud no tiene evidencia registrada de la validación de identidad. Registre la evidencia antes de exportar.");

            return solicitud;
        }

        private async Task ValidarUsuarioAsync(long usuarioId)
        {
            if (!await _solicitudRepositorio.ExisteUsuarioActivoAsync(usuarioId))
                throw new ArgumentException($"No existe un usuario activo con id {usuarioId}.");
        }

        private static List<string> ObtenerSecciones(List<string>? solicitadas)
        {
            if (solicitadas == null || solicitadas.Count == 0)
                return SeccionesExportables.Keys.ToList();

            var elegidas = new HashSet<string>();

            foreach (var valor in solicitadas)
            {
                var seccion = Normalizar(valor ?? string.Empty);

                if (!SeccionesExportables.ContainsKey(seccion))
                    throw new ArgumentException(
                        $"La sección '{valor}' no existe. Use: {string.Join(", ", SeccionesExportables.Keys)}.");

                elegidas.Add(seccion);
            }

            return SeccionesExportables.Keys.Where(elegidas.Contains).ToList();
        }

        private string ObtenerCarpeta()
        {
            var configurada = _configuration["Exportacion:Carpeta"];

            if (!string.IsNullOrWhiteSpace(configurada))
                return configurada.Trim();

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PodoFlow",
                "Exportaciones");
        }

        private string? ResolverRuta(string rutaRegistrada)
        {
            try
            {
                var carpeta = Path.GetFullPath(ObtenerCarpeta());
                var prefijo = Path.EndsInDirectorySeparator(carpeta) ? carpeta : carpeta + Path.DirectorySeparatorChar;
                var ruta = Path.GetFullPath(Path.Combine(carpeta, rutaRegistrada.Trim()));

                return ruta.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase) ? ruta : null;
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
            {
                return null;
            }
        }

        private static byte[] ArmarJson(
            Dictionary<string, object?> metadatos,
            Dictionary<string, List<Dictionary<string, object?>>> datos)
        {
            var documento = new Dictionary<string, object?>
            {
                ["metadatos"] = metadatos,
                ["datos"] = datos
            };

            return JsonSerializer.SerializeToUtf8Bytes(documento, OpcionesJson);
        }

        private byte[] ArmarPdf(
            long solicitudId,
            string codigo,
            DateTime fecha,
            List<string> secciones,
            Dictionary<string, List<Dictionary<string, object?>>> datos)
        {
            var consultorio = _configuration.GetSection("Consultorio").Get<DatosConsultorioDto>() ?? new DatosConsultorioDto();

            if (string.IsNullOrWhiteSpace(consultorio.Nombre))
                consultorio.Nombre = "PodoFlow";

            var huellaDatos = CalcularHuella(JsonSerializer.SerializeToUtf8Bytes(datos, OpcionesJson));

            var documento = new ExportacionPdfDocumento(consultorio, solicitudId, codigo, fecha, secciones, datos, huellaDatos);

            return documento.GeneratePdf();
        }

        private static byte[] ArmarCsv(
            Dictionary<string, object?> metadatos,
            Dictionary<string, List<Dictionary<string, object?>>> datos)
        {
            var texto = new StringBuilder();

            texto.Append("seccion,registro,campo,valor\r\n");

            foreach (var (campo, valor) in metadatos)
                AgregarLineaCsv(texto, "metadatos", 1, campo, valor);

            foreach (var (conjunto, filas) in datos)
                for (var i = 0; i < filas.Count; i++)
                    foreach (var (campo, valor) in filas[i])
                        AgregarLineaCsv(texto, conjunto, i + 1, campo, valor);

            return [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(texto.ToString())];
        }

        private static void AgregarLineaCsv(StringBuilder texto, string seccion, int registro, string campo, object? valor)
        {
            texto.Append(EscaparCsv(seccion))
                .Append(',')
                .Append(registro.ToString(CultureInfo.InvariantCulture))
                .Append(',')
                .Append(EscaparCsv(campo))
                .Append(',')
                .Append(EscaparCsv(FormatearValor(valor)))
                .Append("\r\n");
        }

        private static string FormatearValor(object? valor)
        {
            return valor switch
            {
                null => string.Empty,
                string texto => texto,
                DateTime fecha => fecha.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture),
                DateOnly dia => dia.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                bool logico => logico ? "true" : "false",
                IEnumerable<string> lista => string.Join(" | ", lista),
                IFormattable formateable => formateable.ToString(null, CultureInfo.InvariantCulture),
                _ => valor.ToString() ?? string.Empty
            };
        }

        private static string EscaparCsv(string valor)
        {
            if (valor.Length > 0 && PareceFormula(valor))
                valor = "'" + valor;

            if (valor.IndexOfAny([',', '"', '\r', '\n']) >= 0)
                return "\"" + valor.Replace("\"", "\"\"") + "\"";

            return valor;
        }

        private static bool PareceFormula(string valor)
        {
            var primero = valor[0];

            if (primero is '=' or '@' or '\t' or '\r')
                return true;

            if (primero is '+' or '-')
                return !valor.Skip(1).All(c => char.IsDigit(c) || c is ' ' or '.' or '(' or ')' or '-');

            return false;
        }

        private static string CalcularHuella(byte[] contenido)
        {
            return Convert.ToHexString(SHA256.HashData(contenido)).ToLowerInvariant();
        }

        private static string TipoContenido(string formato)
        {
            return formato switch
            {
                Json => "application/json",
                Csv => "text/csv",
                "PDF" => "application/pdf",
                "ZIP" => "application/zip",
                _ => "application/octet-stream"
            };
        }

        private static DateTime AhoraSinFraccion()
        {
            var ahora = DateTime.UtcNow;

            return new DateTime(ahora.Ticks - ahora.Ticks % TimeSpan.TicksPerSecond, DateTimeKind.Utc);
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