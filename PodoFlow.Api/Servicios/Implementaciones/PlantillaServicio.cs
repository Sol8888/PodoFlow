using PodoFlow.Api.DTOs.Plantilla;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    public class PlantillaServicio : IPlantillaServicio
    {
        private const string Correo = "CORREO";
        private const string WhatsApp = "WHATSAPP";

        private const string VersionInicial = "1.0";

        // Límite de WhatsApp para el cuerpo de un mensaje de texto.
        private const int MaximoWhatsApp = 4096;

        private static readonly HashSet<string> Canales = [Correo, WhatsApp];

        private readonly IPlantillaRepositorio _repositorio;

        public PlantillaServicio(IPlantillaRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<List<PlantillaRespuestaDto>> ObtenerTodosAsync(
            string? canal, string? tipo, bool incluirInactivos, int pagina, int tamano)
        {
            var filtroCanal = string.IsNullOrWhiteSpace(canal) ? null : NormalizarCanal(canal);
            var filtroTipo = string.IsNullOrWhiteSpace(tipo) ? null : Normalizar(tipo);

            var plantillas = await _repositorio.ObtenerTodosAsync(
                filtroCanal, filtroTipo, incluirInactivos, pagina, tamano);

            return plantillas.Select(MapearRespuesta).ToList();
        }

        public async Task<PlantillaRespuestaDto?> ObtenerPorIdAsync(int id)
        {
            var plantilla = await _repositorio.ObtenerPorIdAsync(id);

            return plantilla == null ? null : MapearRespuesta(plantilla);
        }

        public async Task<PlantillaRespuestaDto> CrearAsync(
            PlantillaCrearDto dto)
        {
            var datos = PrepararDatos(dto.Nombre, dto.Canal, dto.Tipo, dto.Asunto, dto.Contenido, dto.Version);

            await ValidarDuplicadoAsync(datos.Nombre, datos.Canal, datos.Version, null);

            var plantilla = new Plantilla
            {
                PlFechaCrea = DateTime.UtcNow,
                PlEstatus = true
            };

            AplicarDatos(plantilla, datos);

            var creada = await _repositorio.CrearAsync(plantilla);

            return MapearRespuesta(creada);
        }

        public async Task<bool> ActualizarAsync(
            int id,
            PlantillaActualizarDto dto)
        {
            var plantilla = await _repositorio.ObtenerPorIdAsync(id);

            if (plantilla == null)
                return false;

            var usos = await _repositorio.ContarNotificacionesAsync(id, soloPendientes: false);

            if (usos > 0)
                throw new InvalidOperationException(
                    $"La plantilla ya se usó en {usos} notificación(es) y no se puede modificar. " +
                    "Cree una versión nueva y dé de baja esta.");

            var datos = PrepararDatos(dto.Nombre, dto.Canal, dto.Tipo, dto.Asunto, dto.Contenido, dto.Version);

            await ValidarDuplicadoAsync(datos.Nombre, datos.Canal, datos.Version, id);

            AplicarDatos(plantilla, datos);

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var plantilla = await _repositorio.ObtenerPorIdAsync(id);

            if (plantilla == null)
                return false;

            if (!plantilla.PlEstatus)
                return true;

            var pendientes = await _repositorio.ContarNotificacionesAsync(id, soloPendientes: true);

            if (pendientes > 0)
                throw new InvalidOperationException(
                    $"Hay {pendientes} notificación(es) pendientes con esta plantilla. " +
                    "Cancélelas o espere a que se envíen antes de darla de baja.");

            plantilla.PlEstatus = false;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        public async Task<bool> ReactivarAsync(int id)
        {
            var plantilla = await _repositorio.ObtenerPorIdAsync(id);

            if (plantilla == null)
                return false;

            if (plantilla.PlEstatus)
                return true;

            // Al reactivarla no puede quedar duplicada con otra que ya esté activa.
            var duplicado = await _repositorio.ObtenerDuplicadoAsync(
                plantilla.PlNombre, plantilla.PlCanal, plantilla.PlVersi, id);

            if (duplicado != null && duplicado.PlEstatus)
                throw new InvalidOperationException(
                    $"Ya hay una plantilla activa igual (id {duplicado.PlId}). Dé de baja esa antes de reactivar esta.");

            plantilla.PlEstatus = true;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }
        private static DatosPlantilla PrepararDatos(
            string nombre, string canal, string tipo, string? asunto, string contenido, string? version)
        {
            var canalNormalizado = NormalizarCanal(canal);
            var tipoNormalizado = Normalizar(tipo);
            var contenidoLimpio = contenido.Trim();

            if (string.IsNullOrEmpty(tipoNormalizado))
                throw new ArgumentException("El tipo es obligatorio.");

            if (string.IsNullOrEmpty(contenidoLimpio))
                throw new ArgumentException("El contenido es obligatorio.");

            var asuntoLimpio = Limpiar(asunto);

            if (canalNormalizado == Correo && asuntoLimpio == null)
                throw new ArgumentException("Las plantillas de CORREO necesitan asunto.");

            if (canalNormalizado == WhatsApp)
            {
                asuntoLimpio = null;

                if (contenidoLimpio.Length > MaximoWhatsApp)
                    throw new ArgumentException(
                        $"El contenido para WHATSAPP no puede pasar de {MaximoWhatsApp} caracteres " +
                        $"(tiene {contenidoLimpio.Length}).");
            }

            return new DatosPlantilla(
                nombre.Trim(),
                canalNormalizado,
                tipoNormalizado,
                asuntoLimpio,
                contenidoLimpio,
                Limpiar(version) ?? VersionInicial);
        }

        private static void AplicarDatos(Plantilla plantilla, DatosPlantilla datos)
        {
            plantilla.PlNombre = datos.Nombre;
            plantilla.PlCanal = datos.Canal;
            plantilla.PlTipo = datos.Tipo;
            plantilla.PlAsunto = datos.Asunto;
            plantilla.PlConte = datos.Contenido;
            plantilla.PlVersi = datos.Version;
        }

        private async Task ValidarDuplicadoAsync(string nombre, string canal, string version, int? excluirId)
        {
            var duplicado = await _repositorio.ObtenerDuplicadoAsync(nombre, canal, version, excluirId);

            if (duplicado == null)
                return;

            var descripcion = $"'{duplicado.PlNombre}' ({duplicado.PlCanal} v{duplicado.PlVersi})";

            if (duplicado.PlEstatus)
                throw new InvalidOperationException(
                    $"Ya existe la plantilla {descripcion} (id {duplicado.PlId}). Use otra versión.");

            throw new InvalidOperationException(
                $"Ya existe la plantilla {descripcion} dada de baja (id {duplicado.PlId}). " +
                $"Reactívela con PATCH api/Plantilla/{duplicado.PlId}/reactivar o use otra versión.");
        }

        private static string NormalizarCanal(string canal)
        {
            var normalizado = Normalizar(canal);

            if (!Canales.Contains(normalizado))
                throw new ArgumentException("El canal debe ser CORREO o WHATSAPP.");

            return normalizado;
        }

        private static string Normalizar(string valor)
        {
            return valor.Trim().ToUpperInvariant();
        }

        private static string? Limpiar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private static PlantillaRespuestaDto MapearRespuesta(
            Plantilla plantilla)
        {
            return new PlantillaRespuestaDto
            {
                Id = plantilla.PlId,
                Nombre = plantilla.PlNombre,
                Canal = plantilla.PlCanal,
                Tipo = plantilla.PlTipo,
                Asunto = plantilla.PlAsunto,
                Contenido = plantilla.PlConte,
                Version = plantilla.PlVersi,
                FechaCreacion = plantilla.PlFechaCrea,
                Estatus = plantilla.PlEstatus
            };
        }

        // Datos ya normalizados y validados, listos para copiarlos a la entidad.
        private sealed record DatosPlantilla(
            string Nombre, string Canal, string Tipo, string? Asunto, string Contenido, string Version);
    }
}
