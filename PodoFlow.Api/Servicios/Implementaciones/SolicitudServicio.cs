using PodoFlow.Api.DTOs.Solicitud;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    public class SolicitudServicio : ISolicitudServicio
    {
        private const string Recibida = "RECIBIDA";
        private const string Validando = "VALIDANDO";
        private const string EnProceso = "EN_PROCESO";
        private const string Respondida = "RESPONDIDA";
        private const string Cerrada = "CERRADA";
        private const string Rechazada = "RECHAZADA";
        private const string Titular = "TITULAR";
        private const string Representante = "REPRESENTANTE";
        private const string Cedula = "CEDULA";
        private const string Pasaporte = "PASAPORTE";
        private static readonly HashSet<string> TiposDerecho =
        [
            "INFORMACION", "ACCESO", "CONSULTA", "RECTIFICACION", "ACTUALIZACION",
            "ELIMINACION", "OPOSICION", "SUSPENSION", "PORTABILIDAD"
        ];

        private static readonly HashSet<string> TiposSolicitante = [Titular, Representante];
        private static readonly HashSet<string> DerechosConExportacion = ["ACCESO", "PORTABILIDAD"];
        private static readonly HashSet<string> TiposIdentificacion = [Cedula, Pasaporte];
        private static readonly Dictionary<string, string[]> TransicionesPermitidas = new()
        {
            [Recibida] = [Validando, Rechazada],
            [Validando] = [EnProceso, Rechazada],
            [EnProceso] = [Respondida, Rechazada],
            [Respondida] = [Cerrada],
            [Cerrada] = [],
            [Rechazada] = []
        };

        private const int PlazoDiasPorDefecto = 15;
        private static readonly HashSet<string> EstadosConPlazo = [Recibida, Validando, EnProceso];

        private readonly ISolicitudRepositorio _repositorio;
        private readonly int _plazoDias;

        public SolicitudServicio(ISolicitudRepositorio repositorio, IConfiguration configuration)
        {
            _repositorio = repositorio;

            var plazo = configuration.GetValue("Solicitudes:PlazoDias", PlazoDiasPorDefecto);
            _plazoDias = plazo > 0 ? plazo : PlazoDiasPorDefecto;
        }

        public async Task<List<SolicitudRespuestaDto>> ObtenerTodosAsync(int pagina, int tamano)
        {
            var solicitudes = await _repositorio.ObtenerTodosAsync(pagina, tamano);

            return solicitudes.Select(MapearRespuesta).ToList();
        }

        public async Task<List<SolicitudRespuestaDto>> ObtenerPorPacienteAsync(long pacienteId)
        {
            var solicitudes = await _repositorio.ObtenerPorPacienteAsync(pacienteId);

            return solicitudes.Select(MapearRespuesta).ToList();
        }

        public async Task<SolicitudRespuestaDto?> ObtenerPorIdAsync(long id)
        {
            var solicitud = await _repositorio.ObtenerPorIdAsync(id);

            if (solicitud == null)
                return null;

            return MapearRespuesta(solicitud);
        }

        public async Task<List<AccionSolicitudRespuestaDto>?> ObtenerAccionesAsync(long id)
        {
            var solicitud = await _repositorio.ObtenerPorIdAsync(id);

            if (solicitud == null)
                return null;

            var acciones = await _repositorio.ObtenerAccionesAsync(id);

            return acciones.Select(MapearAccion).ToList();
        }

        public async Task<SolicitudRespuestaDto> CrearAsync(
            SolicitudCrearDto dto)
        {
            var tipoDerecho = Normalizar(dto.TipoDerecho);
            var tipoSolicitante = Normalizar(dto.TipoSolicitante);

            if (!TiposDerecho.Contains(tipoDerecho))
                throw new ArgumentException(
                    $"Tipo de derecho inválido. Valores permitidos: {string.Join(", ", TiposDerecho)}.");

            if (!TiposSolicitante.Contains(tipoSolicitante))
                throw new ArgumentException("El tipo de solicitante debe ser TITULAR o REPRESENTANTE.");

            if (!await _repositorio.ExistePacienteActivoAsync(dto.PacienteId))
                throw new ArgumentException($"No existe un paciente activo con id {dto.PacienteId}.");

            if (dto.UsuarioId.HasValue)
                await ValidarUsuarioAsync(dto.UsuarioId.Value);

            var identificacion = await ValidarSolicitanteAsync(dto.PacienteId, tipoSolicitante, dto.IdentificacionSolicitante, dto.TipoIdentificacion);

            var ahora = DateTime.UtcNow;

            var solicitud = new Solicitud
            {
                PaId = dto.PacienteId,
                UsId = dto.UsuarioId,
                SoTipoDere = tipoDerecho,
                SoTipoSoli = tipoSolicitante,
                SoNombrSoli = dto.NombreSolicitante.Trim(),
                SoIdentSoli = identificacion,
                SoFechaRece = ahora,
                SoEstad = Recibida,
                SoMedioResp = dto.MedioRespuesta?.Trim(),
                SoDetal = dto.Detalle?.Trim()
            };

            AccionSolicitud? accion = null;

            if (dto.UsuarioId.HasValue)
            {
                accion = new AccionSolicitud
                {
                    UsId = dto.UsuarioId.Value,
                    AcFechaHora = ahora,
                    AcEstadAnte = null,
                    AcEstadNuev = Recibida,
                    AcAccion = "Solicitud registrada"
                };
            }

            var creada = await _repositorio.CrearAsync(solicitud, accion);

            return MapearRespuesta(creada);
        }

        public async Task<bool> ActualizarAsync(
            long id,
            SolicitudActualizarDto dto)
        {
            var solicitud = await _repositorio.ObtenerPorIdAsync(id);

            if (solicitud == null)
                return false;

            if (EsFinal(solicitud.SoEstad))
                throw new InvalidOperationException(
                    $"La solicitud está {solicitud.SoEstad} y ya no se puede modificar.");
            var identificacion = await ValidarSolicitanteAsync(
                solicitud.PaId, solicitud.SoTipoSoli,
                dto.IdentificacionSolicitante, dto.TipoIdentificacion);

            solicitud.SoNombrSoli = dto.NombreSolicitante.Trim();
            solicitud.SoIdentSoli = identificacion;
            solicitud.SoMedioResp = dto.MedioRespuesta?.Trim();
            solicitud.SoDetal = dto.Detalle?.Trim();

            await _repositorio.ActualizarAsync(solicitud, dto.Version);

            return true;
        }

        public async Task<SolicitudRespuestaDto?> CambiarEstadoAsync(
            long id,
            SolicitudCambiarEstadoDto dto)
        {
            var nuevoEstado = Normalizar(dto.Estado);

            if (!TransicionesPermitidas.ContainsKey(nuevoEstado))
                throw new ArgumentException(
                    "El estado debe ser RECIBIDA, VALIDANDO, EN_PROCESO, RESPONDIDA, CERRADA o RECHAZADA.");

            var solicitud = await _repositorio.ObtenerPorIdAsync(id);

            if (solicitud == null)
                return null;

            var estadoAnterior = solicitud.SoEstad;

            if (!TransicionesPermitidas[estadoAnterior].Contains(nuevoEstado))
                throw new InvalidOperationException(
                    $"No se puede pasar de {estadoAnterior} a {nuevoEstado}.");

            await ValidarUsuarioAsync(dto.UsuarioId);

            if (nuevoEstado == Respondida
                && DerechosConExportacion.Contains(solicitud.SoTipoDere)
                && !await _repositorio.ExisteExportacionEntregadaAsync(solicitud.SoId))
                throw new InvalidOperationException(
                    $"Una solicitud de {solicitud.SoTipoDere} se responde entregando los datos al titular. Genere la exportación y márquela como ENTREGADA antes de responder.");

            var ahora = DateTime.UtcNow;

            if (nuevoEstado == Respondida || nuevoEstado == Rechazada)
            {
                if (string.IsNullOrWhiteSpace(dto.Respuesta))
                    throw new ArgumentException(nuevoEstado == Respondida
                        ? "Para marcarla como RESPONDIDA hay que enviar la respuesta."
                        : "Para RECHAZAR la solicitud hay que indicar el motivo en la respuesta.");

                solicitud.SoRespue = dto.Respuesta.Trim();

                if (!string.IsNullOrWhiteSpace(dto.MedioRespuesta))
                    solicitud.SoMedioResp = dto.MedioRespuesta.Trim();

                if (nuevoEstado == Respondida && string.IsNullOrWhiteSpace(solicitud.SoMedioResp))
                    throw new ArgumentException("Hay que indicar el medio por el que se respondió.");
            }

            if (EsFinal(nuevoEstado))
                solicitud.SoFechaCier = ahora;

            solicitud.SoEstad = nuevoEstado;

            var accion = new AccionSolicitud
            {
                SoId = solicitud.SoId,
                UsId = dto.UsuarioId,
                AcFechaHora = ahora,
                AcEstadAnte = estadoAnterior,
                AcEstadNuev = nuevoEstado,
                AcAccion = string.IsNullOrWhiteSpace(dto.Accion)
                    ? $"Cambio de estado de {estadoAnterior} a {nuevoEstado}"
                    : dto.Accion.Trim(),
                AcObser = dto.Observacion?.Trim()
            };

            await _repositorio.CambiarEstadoAsync(solicitud, accion, dto.Version);

            return MapearRespuesta(solicitud);
        }

        private async Task ValidarUsuarioAsync(long usuarioId)
        {
            if (!await _repositorio.ExisteUsuarioActivoAsync(usuarioId))
                throw new ArgumentException($"No existe un usuario activo con id {usuarioId}.");
        }

        private async Task<string> ValidarSolicitanteAsync(
            long pacienteId, string tipoSolicitante,
            string identificacion, string? tipoIdentificacion)
        {
            var paciente = await _repositorio.ObtenerIdentidadPacienteAsync(pacienteId)
                ?? throw new ArgumentException($"No existe un paciente con id {pacienteId}.");

            var identificacionLimpia = LimpiarIdentificacion(identificacion);

            if (tipoSolicitante == Titular)
                ValidarTitular(identificacionLimpia, paciente);
            else
                ValidarRepresentante(identificacionLimpia, tipoIdentificacion, paciente.Identificacion);

            return identificacionLimpia;
        }

        private static void ValidarTitular(
            string identificacion,
            (string TipoIdentificacion, string Identificacion, string Nombre) paciente)
        {
            var tipo = Normalizar(paciente.TipoIdentificacion);

            if (!TiposIdentificacion.Contains(tipo))
                throw new ArgumentException(
                    $"El paciente está registrado con {tipo}; el titular solo puede presentar solicitudes con cédula o pasaporte.");

            ValidarFormato(identificacion, tipo);

            if (identificacion != LimpiarIdentificacion(paciente.Identificacion))
                throw new ArgumentException("La identificación del titular no coincide con la del paciente.");
        }

        // REPRESENTANTE: tipo obligatorio, formato válido y distinto del paciente.
        private static void ValidarRepresentante(
            string identificacion, string? tipoIdentificacion, string identificacionPaciente)
        {
            if (string.IsNullOrWhiteSpace(tipoIdentificacion))
                throw new ArgumentException(
                    "Para un REPRESENTANTE hay que indicar el tipo de identificación: CEDULA o PASAPORTE.");

            var tipo = Normalizar(tipoIdentificacion);

            if (!TiposIdentificacion.Contains(tipo))
                throw new ArgumentException("El tipo de identificación del representante debe ser CEDULA o PASAPORTE.");

            ValidarFormato(identificacion, tipo);

            if (identificacion == LimpiarIdentificacion(identificacionPaciente))
                throw new ArgumentException(
                    "El representante no puede tener la misma identificación que el paciente. Si es el propio paciente, use TITULAR.");
        }

        private static void ValidarFormato(string identificacion, string tipo)
        {
            if (tipo == Cedula && !EsCedulaValida(identificacion))
                throw new ArgumentException("La identificación debe ser una cédula ecuatoriana válida.");

            if (tipo == Pasaporte && !EsPasaporteValido(identificacion))
                throw new ArgumentException("El pasaporte debe tener entre 6 y 9 letras o números.");
        }

        // Quita espacios y guiones y pasa a mayúsculas (los pasaportes llevan letras).
        private static string LimpiarIdentificacion(string valor)
        {
            return valor.Trim()
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty)
                .ToUpperInvariant();
        }

        // 10 dígitos, provincia 01–24 o 30, tercer dígito menor a 6 y verificador módulo 10.
        private static bool EsCedulaValida(string cedula)
        {
            if (cedula.Length != 10 || !cedula.All(char.IsAsciiDigit))
                return false;

            var provincia = int.Parse(cedula[..2]);

            if (provincia is not ((>= 1 and <= 24) or 30))
                return false;

            if (cedula[2] - '0' >= 6)
                return false;

            var suma = 0;

            for (var i = 0; i < 9; i++)
            {
                var producto = (cedula[i] - '0') * (i % 2 == 0 ? 2 : 1);
                suma += producto > 9 ? producto - 9 : producto;
            }

            var verificador = (10 - suma % 10) % 10;

            return verificador == cedula[9] - '0';
        }

        // 6 a 9 caracteres alfanuméricos (ya limpio y en mayúsculas).
        private static bool EsPasaporteValido(string pasaporte)
        {
            return pasaporte.Length is >= 6 and <= 9
                && pasaporte.All(char.IsAsciiLetterOrDigit);
        }

        private static bool EsFinal(string estado)
        {
            return estado == Cerrada || estado == Rechazada;
        }

        private static string Normalizar(string valor)
        {
            return valor.Trim().ToUpperInvariant();
        }

        private SolicitudRespuestaDto MapearRespuesta(
            Solicitud solicitud)
        {
            var fechaLimite = DateTime.SpecifyKind(solicitud.SoFechaRece, DateTimeKind.Utc).AddDays(_plazoDias);
            var abierta = EstadosConPlazo.Contains(solicitud.SoEstad);
            var ahora = DateTime.UtcNow;

            return new SolicitudRespuestaDto
            {
                Id = solicitud.SoId,
                PacienteId = solicitud.PaId,
                UsuarioId = solicitud.UsId,
                TipoDerecho = solicitud.SoTipoDere,
                TipoSolicitante = solicitud.SoTipoSoli,
                NombreSolicitante = solicitud.SoNombrSoli,
                IdentificacionSolicitante = solicitud.SoIdentSoli,
                FechaRecepcion = solicitud.SoFechaRece,
                Estado = solicitud.SoEstad,
                MedioRespuesta = solicitud.SoMedioResp,
                Detalle = solicitud.SoDetal,
                Respuesta = solicitud.SoRespue,
                FechaCierre = solicitud.SoFechaCier,
                FechaLimite = fechaLimite,
                DiasRestantes = abierta ? (int)Math.Ceiling((fechaLimite - ahora).TotalDays) : null,
                Vencida = abierta && ahora > fechaLimite,
                Version = solicitud.SoVersion
            };
        }

        private static AccionSolicitudRespuestaDto MapearAccion(
            AccionSolicitud accion)
        {
            return new AccionSolicitudRespuestaDto
            {
                Id = accion.AcId,
                SolicitudId = accion.SoId,
                UsuarioId = accion.UsId,
                FechaHora = accion.AcFechaHora,
                EstadoAnterior = accion.AcEstadAnte,
                EstadoNuevo = accion.AcEstadNuev,
                Accion = accion.AcAccion,
                Observacion = accion.AcObser
            };
        }
    }
}