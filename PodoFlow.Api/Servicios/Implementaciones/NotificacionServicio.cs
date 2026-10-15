using PodoFlow.Api.DTOs.Notificacion;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    // Caso B: la "baja" es CANCELAR, y solo si todavía no se envió.
    // La API no envía mensajes: un proceso externo la pasa a PROCESANDO (aquí se vuelve a verificar
    // el permiso y el consentimiento), envía y registra el intento en ENVIO, que la deja ENVIADA o ERROR.
    // Convención: null/false → 404 · ArgumentException → 400 · InvalidOperationException → 409.
    public class NotificacionServicio : INotificacionServicio
    {
        private const string Pendiente = "PENDIENTE";
        private const string Procesando = "PROCESANDO";
        private const string Enviada = "ENVIADA";
        private const string Error = "ERROR";
        private const string Cancelada = "CANCELADA";

        // Resultados de ENVIO (CK_ENVIO_result).
        private const string ResultadoEnviado = "ENVIADO";
        private const string ResultadoError = "ERROR";

        private const string Aceptado = "ACEPTADO";

        private const string MensajeEstadoInvalido =
            "El estado debe ser PENDIENTE, PROCESANDO, ENVIADA, ERROR o CANCELADA.";

        // Todas las transiciones válidas. ENVIADA y CANCELADA son finales.
        // ERROR → PENDIENTE es el reintento; ERROR → CANCELADA, desistir.
        private static readonly Dictionary<string, string[]> TransicionesPermitidas = new()
        {
            [Pendiente] = [Procesando, Cancelada],
            [Procesando] = [Enviada, Error],
            [Error] = [Pendiente, Cancelada],
            [Enviada] = [],
            [Cancelada] = []
        };

        // ENVIADA y ERROR solo salen de registrar un intento, para que siempre quede su ENVIO.
        private static readonly HashSet<string> EstadosPorEnvio = [Enviada, Error];

        // Se puede reprogramar o cambiar de plantilla mientras no haya salido.
        private static readonly HashSet<string> EstadosEditables = [Pendiente, Error];

        private readonly INotificacionRepositorio _repositorio;
        private readonly IOpcionComunicacionRepositorio _opcionRepositorio;
        private readonly IFinalidadRepositorio _finalidadRepositorio;
        private readonly IPlantillaRepositorio _plantillaRepositorio;

        public NotificacionServicio(
            INotificacionRepositorio repositorio,
            IOpcionComunicacionRepositorio opcionRepositorio,
            IFinalidadRepositorio finalidadRepositorio,
            IPlantillaRepositorio plantillaRepositorio)
        {
            _repositorio = repositorio;
            _opcionRepositorio = opcionRepositorio;
            _finalidadRepositorio = finalidadRepositorio;
            _plantillaRepositorio = plantillaRepositorio;
        }

        public async Task<List<NotificacionRespuestaDto>> ObtenerTodosAsync(
            long? pacienteId, string? estado, int pagina, int tamano)
        {
            string? filtroEstado = null;

            if (!string.IsNullOrWhiteSpace(estado))
            {
                filtroEstado = Normalizar(estado);

                if (!TransicionesPermitidas.ContainsKey(filtroEstado))
                    throw new ArgumentException(MensajeEstadoInvalido);
            }

            var notificaciones = await _repositorio.ObtenerTodosAsync(pacienteId, filtroEstado, pagina, tamano);

            return notificaciones.Select(MapearRespuesta).ToList();
        }

        public async Task<List<NotificacionRespuestaDto>> ObtenerPorEnviarAsync(int tamano)
        {
            var notificaciones = await _repositorio.ObtenerPorEnviarAsync(DateTime.UtcNow, tamano);

            return notificaciones.Select(MapearRespuesta).ToList();
        }

        public async Task<NotificacionRespuestaDto?> ObtenerPorIdAsync(long id)
        {
            var notificacion = await _repositorio.ObtenerPorIdAsync(id);

            return notificacion == null ? null : MapearRespuesta(notificacion);
        }

        public async Task<List<EnvioRespuestaDto>?> ObtenerEnviosAsync(long id)
        {
            var notificacion = await _repositorio.ObtenerPorIdAsync(id);

            if (notificacion == null)
                return null;

            var envios = await _repositorio.ObtenerEnviosAsync(id);

            return envios.Select(MapearEnvio).ToList();
        }

        public async Task<NotificacionRespuestaDto> CrearAsync(
            NotificacionCrearDto dto)
        {
            var fechaProgramada = ValidarFechaProgramada(dto.FechaProgramada);

            var opcion = await _opcionRepositorio.ObtenerPorIdAsync(dto.OpcionId);

            if (opcion == null || !opcion.OpEstatus)
                throw new ArgumentException($"No existe una opción de comunicación activa con id {dto.OpcionId}.");

            await VerificarAutorizacionAsync(opcion);

            var plantilla = await ObtenerPlantillaValidaAsync(dto.PlantillaId, opcion.OpCanal);

            if (dto.CitaId.HasValue)
                await ValidarCitaAsync(dto.CitaId.Value, opcion.PaId, fechaProgramada);

            if (dto.UsuarioId.HasValue && !await _repositorio.ExisteUsuarioActivoAsync(dto.UsuarioId.Value))
                throw new ArgumentException($"No existe un usuario activo con id {dto.UsuarioId.Value}.");

            var notificacion = new Notificacion
            {
                PaId = opcion.PaId,
                CiId = dto.CitaId,
                PlId = plantilla.PlId,
                OpId = opcion.OpId,
                UsId = dto.UsuarioId,
                NoTipo = ObtenerTipo(dto.Tipo, plantilla),
                NoCanal = opcion.OpCanal,
                NoFechaProg = fechaProgramada,
                NoEstad = Pendiente,
                NoFechaCrea = DateTime.UtcNow
            };

            var creada = await _repositorio.CrearAsync(notificacion);

            return MapearRespuesta(creada);
        }

        public async Task<bool> ActualizarAsync(
            long id,
            NotificacionActualizarDto dto)
        {
            var notificacion = await _repositorio.ObtenerPorIdAsync(id);

            if (notificacion == null)
                return false;

            if (!EstadosEditables.Contains(notificacion.NoEstad))
                throw new InvalidOperationException(
                    $"La notificación está {notificacion.NoEstad} y ya no se puede modificar.");

            var fechaProgramada = ValidarFechaProgramada(dto.FechaProgramada);

            var plantilla = await ObtenerPlantillaValidaAsync(dto.PlantillaId, notificacion.NoCanal);

            if (notificacion.CiId.HasValue)
                await ValidarCitaAsync(notificacion.CiId.Value, notificacion.PaId, fechaProgramada);

            // Nunca se tocan NoId, PaId, CiId, OpId, UsId, NoCanal, NoEstad ni NoFechaCrea.
            notificacion.PlId = plantilla.PlId;
            notificacion.NoTipo = ObtenerTipo(dto.Tipo, plantilla);
            notificacion.NoFechaProg = fechaProgramada;

            await _repositorio.ActualizarAsync(notificacion, dto.Version);

            return true;
        }

        public async Task<NotificacionRespuestaDto?> CambiarEstadoAsync(
            long id,
            NotificacionCambiarEstadoDto dto)
        {
            var nuevoEstado = Normalizar(dto.Estado);

            if (!TransicionesPermitidas.ContainsKey(nuevoEstado))
                throw new ArgumentException(MensajeEstadoInvalido);

            if (EstadosPorEnvio.Contains(nuevoEstado))
                throw new ArgumentException(
                    $"El estado {nuevoEstado} se asigna al registrar el intento en POST api/Notificacion/{id}/envios.");

            var notificacion = await _repositorio.ObtenerPorIdAsync(id);

            if (notificacion == null)
                return null;

            var estadoAnterior = notificacion.NoEstad;

            if (!TransicionesPermitidas[estadoAnterior].Contains(nuevoEstado))
            {
                if (nuevoEstado == Cancelada)
                    throw new InvalidOperationException(estadoAnterior == Procesando
                        ? "La notificación se está enviando y ya no se puede cancelar."
                        : $"La notificación está {estadoAnterior} y ya no se puede cancelar.");

                throw new InvalidOperationException($"No se puede pasar de {estadoAnterior} a {nuevoEstado}.");
            }

            // Justo antes de enviar se vuelve a verificar todo: desde que se programó,
            // el paciente pudo revocar el consentimiento, quitar el permiso o cancelar la cita.
            if (nuevoEstado == Procesando)
                await VerificarAntesDeEnviarAsync(notificacion);

            notificacion.NoEstad = nuevoEstado;

            await _repositorio.ActualizarAsync(notificacion, dto.Version);

            return MapearRespuesta(notificacion);
        }

        public async Task<EnvioRespuestaDto?> RegistrarEnvioAsync(
            long id,
            EnvioCrearDto dto)
        {
            var resultado = Normalizar(dto.Resultado);

            if (resultado != ResultadoEnviado && resultado != ResultadoError)
                throw new ArgumentException("El resultado debe ser ENVIADO o ERROR.");

            var codigoError = Limpiar(dto.CodigoError);
            var detalleError = Limpiar(dto.DetalleError);

            if (resultado == ResultadoError && codigoError == null && detalleError == null)
                throw new ArgumentException("Si el resultado es ERROR hay que indicar el código o el detalle del error.");

            var notificacion = await _repositorio.ObtenerPorIdAsync(id);

            if (notificacion == null)
                return null;

            // Solo se registran intentos de lo que el proceso de envío tomó.
            if (notificacion.NoEstad != Procesando)
                throw new InvalidOperationException(
                    $"La notificación está {notificacion.NoEstad}. Solo se registran intentos de notificaciones en PROCESANDO.");

            notificacion.NoEstad = resultado == ResultadoEnviado ? Enviada : Error;

            var envio = new Envio
            {
                NoId = notificacion.NoId,
                EnFechaHora = DateTime.UtcNow,
                EnResul = resultado,
                EnIdProve = Limpiar(dto.IdProveedor),
                // En un envío correcto no se guardan datos de error.
                EnCodError = resultado == ResultadoError ? codigoError : null,
                EnDetalError = resultado == ResultadoError ? detalleError : null
            };

            var creado = await _repositorio.RegistrarEnvioAsync(notificacion, envio);

            return MapearEnvio(creado);
        }

        // Paciente activo, opción activa y con permiso, finalidad activa y, si la finalidad lo pide,
        // consentimiento ACEPTADO. Es la misma cadena que exige OPCION_COMUNICACION.
        private async Task VerificarAutorizacionAsync(OpcionComunicacion opcion)
        {
            if (!await _opcionRepositorio.ExistePacienteActivoAsync(opcion.PaId))
                throw new InvalidOperationException($"El paciente {opcion.PaId} ya no está activo.");

            if (!opcion.OpEstatus)
                throw new InvalidOperationException($"La opción de comunicación {opcion.OpId} está dada de baja.");

            if (!opcion.OpPermi)
                throw new InvalidOperationException(
                    $"El paciente no autorizó mensajes por {opcion.OpCanal} para esta finalidad (opción {opcion.OpId}).");

            var finalidad = await _finalidadRepositorio.ObtenerPorIdAsync(opcion.FiId);

            if (finalidad == null || !finalidad.FiEstatus)
                throw new InvalidOperationException($"La finalidad {opcion.FiId} de la opción ya no está activa.");

            if (!finalidad.FiRequiCons)
                return;

            var consentimiento = await _opcionRepositorio.ObtenerConsentimientoAsync(opcion.PaId, opcion.FiId);

            if (consentimiento == null || consentimiento.CoEstad != Aceptado)
                throw new InvalidOperationException(
                    $"La finalidad {finalidad.FiCodigo} requiere consentimiento y el paciente " +
                    (consentimiento == null ? "no lo tiene registrado." : $"lo tiene {consentimiento.CoEstad}."));
        }

        private async Task VerificarAntesDeEnviarAsync(Notificacion notificacion)
        {
            const string sugerencia = " Cancele la notificación.";

            try
            {
                if (!notificacion.OpId.HasValue)
                    throw new InvalidOperationException("La notificación no tiene opción de comunicación.");

                var opcion = await _opcionRepositorio.ObtenerPorIdAsync(notificacion.OpId.Value);

                if (opcion == null)
                    throw new InvalidOperationException($"No existe la opción de comunicación {notificacion.OpId}.");

                await VerificarAutorizacionAsync(opcion);

                var plantilla = await _plantillaRepositorio.ObtenerPorIdAsync(notificacion.PlId);

                if (plantilla == null || !plantilla.PlEstatus)
                    throw new InvalidOperationException($"La plantilla {notificacion.PlId} ya no está activa.");

                if (notificacion.CiId.HasValue
                    && await _repositorio.ObtenerFechaCitaVigenteAsync(notificacion.CiId.Value, notificacion.PaId) == null)
                    throw new InvalidOperationException($"La cita {notificacion.CiId} ya no está vigente.");
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("No se puede enviar: " + ex.Message + sugerencia);
            }
        }

        private async Task<Plantilla> ObtenerPlantillaValidaAsync(int plantillaId, string canal)
        {
            var plantilla = await _plantillaRepositorio.ObtenerPorIdAsync(plantillaId);

            if (plantilla == null || !plantilla.PlEstatus)
                throw new ArgumentException($"No existe una plantilla activa con id {plantillaId}.");

            // Una plantilla de correo no sirve para WhatsApp (asunto, formato y largo distintos).
            if (plantilla.PlCanal != canal)
                throw new ArgumentException(
                    $"La plantilla {plantillaId} es de {plantilla.PlCanal} y la opción del paciente es por {canal}.");

            return plantilla;
        }

        // La cita tiene que ser del mismo paciente, seguir vigente, y el recordatorio debe salir antes.
        private async Task ValidarCitaAsync(long citaId, long pacienteId, DateTime fechaProgramada)
        {
            var fechaCita = await _repositorio.ObtenerFechaCitaVigenteAsync(citaId, pacienteId);

            if (fechaCita == null)
                throw new ArgumentException(
                    $"No existe una cita vigente (SOLICITADA, CONFIRMADA o REPROGRAMADA) con id {citaId} para este paciente.");

            if (fechaProgramada >= fechaCita.Value)
                throw new ArgumentException(
                    $"La notificación debe programarse antes de la cita ({fechaCita.Value:yyyy-MM-dd HH:mm} UTC).");
        }

        // No se programa en el pasado (5 minutos de margen por diferencias de reloj).
        // Fechas en UTC como el resto de la API: con zona horaria se convierte; sin zona se toma como UTC.
        private static DateTime ValidarFechaProgramada(DateTime? fecha)
        {
            if (!fecha.HasValue)
                throw new ArgumentException("La fecha programada es obligatoria.");

            var valor = fecha.Value.Kind == DateTimeKind.Local
                ? fecha.Value.ToUniversalTime()
                : DateTime.SpecifyKind(fecha.Value, DateTimeKind.Utc);

            if (valor < DateTime.UtcNow.AddMinutes(-5))
                throw new ArgumentException("La fecha programada no puede estar en el pasado.");

            // no_fecha_prog es DATETIME2(0): sin fracciones de segundo.
            return valor.AddTicks(-(valor.Ticks % TimeSpan.TicksPerSecond));
        }

        private static string ObtenerTipo(string? tipo, Plantilla plantilla)
        {
            return string.IsNullOrWhiteSpace(tipo) ? plantilla.PlTipo : Normalizar(tipo);
        }

        private static string Normalizar(string valor)
        {
            return valor.Trim().ToUpperInvariant();
        }

        // Texto opcional: vacío o solo espacios se guarda como NULL.
        private static string? Limpiar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private static NotificacionRespuestaDto MapearRespuesta(
            Notificacion notificacion)
        {
            return new NotificacionRespuestaDto
            {
                Id = notificacion.NoId,
                PacienteId = notificacion.PaId,
                CitaId = notificacion.CiId,
                PlantillaId = notificacion.PlId,
                OpcionId = notificacion.OpId,
                UsuarioId = notificacion.UsId,
                Tipo = notificacion.NoTipo,
                Canal = notificacion.NoCanal,
                FechaProgramada = notificacion.NoFechaProg,
                Estado = notificacion.NoEstad,
                FechaCreacion = notificacion.NoFechaCrea,
                Version = notificacion.NoVersion
            };
        }

        private static EnvioRespuestaDto MapearEnvio(
            Envio envio)
        {
            return new EnvioRespuestaDto
            {
                Id = envio.EnId,
                NotificacionId = envio.NoId,
                NumeroIntento = envio.EnNumInten,
                FechaHora = envio.EnFechaHora,
                Resultado = envio.EnResul,
                IdProveedor = envio.EnIdProve,
                CodigoError = envio.EnCodError,
                DetalleError = envio.EnDetalError
            };
        }
    }
}
