using PodoFlow.Api.DTOs.Incidente;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    public class IncidenteServicio : IIncidenteServicio
    {
        private const string Abierto = "ABIERTO";
        private const string Analisis = "ANALISIS";
        private const string Contenido = "CONTENIDO";
        private const string Recuperado = "RECUPERADO";
        private const string Cerrado = "CERRADO";
        private const string TipoRegistro = "REGISTRO";
        private const string TipoCambioEstado = "CAMBIO_ESTADO";

        private const string MensajeEstadoInvalido =
            "El estado debe ser ABIERTO, ANALISIS, CONTENIDO, RECUPERADO o CERRADO.";
        private static readonly Dictionary<string, string[]> TransicionesPermitidas = new()
        {
            [Abierto] = [Analisis, Contenido, Cerrado],
            [Analisis] = [Contenido, Cerrado],
            [Contenido] = [Recuperado],
            [Recuperado] = [Cerrado],
            [Cerrado] = []
        };

        private readonly IIncidenteRepositorio _repositorio;

        public IncidenteServicio(IIncidenteRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<List<IncidenteRespuestaDto>> ObtenerTodosAsync(string? estado, int pagina, int tamano)
        {
            string? filtroEstado = null;

            if (!string.IsNullOrWhiteSpace(estado))
            {
                filtroEstado = Normalizar(estado);

                if (!TransicionesPermitidas.ContainsKey(filtroEstado))
                    throw new ArgumentException(MensajeEstadoInvalido);
            }

            var incidentes = await _repositorio.ObtenerTodosAsync(filtroEstado, pagina, tamano);

            return incidentes.Select(MapearRespuesta).ToList();
        }

        public async Task<IncidenteRespuestaDto?> ObtenerPorIdAsync(long id)
        {
            var incidente = await _repositorio.ObtenerPorIdAsync(id);

            return incidente == null ? null : MapearRespuesta(incidente);
        }

        public async Task<List<AccionIncidenteRespuestaDto>?> ObtenerAccionesAsync(long id)
        {
            var incidente = await _repositorio.ObtenerPorIdAsync(id);

            if (incidente == null)
                return null;

            var acciones = await _repositorio.ObtenerAccionesAsync(id);

            return acciones.Select(MapearAccion).ToList();
        }

        public async Task<IncidenteRespuestaDto> CrearAsync(
            IncidenteCrearDto dto)
        {
            var fechaDeteccion = ValidarFechaDeteccion(dto.FechaDeteccion);

            await ValidarUsuarioAsync(dto.UsuarioId);

            var ahora = DateTime.UtcNow;

            var incidente = new Incidente
            {
                UsId = dto.UsuarioId,
                InFechaDete = fechaDeteccion,
                InTipo = Normalizar(dto.Tipo),
                InModulo = Normalizar(dto.Modulo),
                InDescri = dto.Descripcion.Trim(),
                InDatosAfec = Limpiar(dto.DatosAfectados),
                InImpacPreli = Limpiar(dto.ImpactoPreliminar),
                InEstad = Abierto
            };

            var accion = new AccionIncidente
            {
                UsId = dto.UsuarioId,
                AcFechaHora = ahora,
                AcTipo = TipoRegistro,
                AcAccion = "Incidente registrado"
            };

            var creado = await _repositorio.CrearAsync(incidente, accion);

            return MapearRespuesta(creado);
        }

        public async Task<bool> ActualizarAsync(
            long id,
            IncidenteActualizarDto dto)
        {
            var incidente = await _repositorio.ObtenerPorIdAsync(id);

            if (incidente == null)
                return false;

            if (incidente.InEstad == Cerrado)
                throw new InvalidOperationException("El incidente está CERRADO y ya no se puede modificar.");

            var fechaDeteccion = ValidarFechaDeteccion(dto.FechaDeteccion);

            incidente.InFechaDete = fechaDeteccion;
            incidente.InTipo = Normalizar(dto.Tipo);
            incidente.InModulo = Normalizar(dto.Modulo);
            incidente.InDescri = dto.Descripcion.Trim();
            incidente.InDatosAfec = Limpiar(dto.DatosAfectados);
            incidente.InImpacPreli = Limpiar(dto.ImpactoPreliminar);

            await _repositorio.ActualizarAsync(incidente, dto.Version);

            return true;
        }

        public async Task<IncidenteRespuestaDto?> CambiarEstadoAsync(
            long id,
            IncidenteCambiarEstadoDto dto)
        {
            var nuevoEstado = Normalizar(dto.Estado);

            if (!TransicionesPermitidas.ContainsKey(nuevoEstado))
                throw new ArgumentException(MensajeEstadoInvalido);

            var incidente = await _repositorio.ObtenerPorIdAsync(id);

            if (incidente == null)
                return null;

            var estadoAnterior = incidente.InEstad;

            if (!TransicionesPermitidas[estadoAnterior].Contains(nuevoEstado))
                throw new InvalidOperationException(
                    $"No se puede pasar de {estadoAnterior} a {nuevoEstado}.");

            // El cierre tiene que quedar justificado en la bitácora (resumen o motivo del descarte).
            if (nuevoEstado == Cerrado && string.IsNullOrWhiteSpace(dto.Observacion))
                throw new ArgumentException(
                    "Para CERRAR el incidente hay que indicar en la observación el resumen del cierre o el motivo del descarte.");

            await ValidarUsuarioAsync(dto.UsuarioId);

            var ahora = DateTime.UtcNow;

            if (nuevoEstado == Cerrado)
                incidente.InFechaCier = ahora;

            incidente.InEstad = nuevoEstado;

            var accion = new AccionIncidente
            {
                InId = incidente.InId,
                UsId = dto.UsuarioId,
                AcFechaHora = ahora,
                AcTipo = TipoCambioEstado,
                AcAccion = string.IsNullOrWhiteSpace(dto.Accion)
                    ? $"Cambio de estado de {estadoAnterior} a {nuevoEstado}"
                    : dto.Accion.Trim(),
                AcObser = Limpiar(dto.Observacion)
            };

            await _repositorio.CambiarEstadoAsync(incidente, accion, dto.Version);

            return MapearRespuesta(incidente);
        }

        public async Task<AccionIncidenteRespuestaDto?> RegistrarAccionAsync(
            long id,
            AccionIncidenteCrearDto dto)
        {
            var incidente = await _repositorio.ObtenerPorIdAsync(id);

            if (incidente == null)
                return null;

            if (incidente.InEstad == Cerrado)
                throw new InvalidOperationException(
                    "El incidente está CERRADO; ya no se le pueden registrar acciones.");

            var tipo = Normalizar(dto.Tipo);

            if (string.IsNullOrEmpty(tipo))
                throw new ArgumentException("El tipo de acción es obligatorio.");

            if (tipo == TipoRegistro || tipo == TipoCambioEstado)
                throw new ArgumentException(
                    $"El tipo {tipo} lo registra el sistema. Para cambiar el estado use PATCH api/Incidente/{id}/estado.");

            if (string.IsNullOrWhiteSpace(dto.Accion))
                throw new ArgumentException("La descripción de la acción es obligatoria.");

            await ValidarUsuarioAsync(dto.UsuarioId);

            var accion = new AccionIncidente
            {
                InId = incidente.InId,
                UsId = dto.UsuarioId,
                AcFechaHora = DateTime.UtcNow,
                AcTipo = tipo,
                AcAccion = dto.Accion.Trim(),
                AcObser = Limpiar(dto.Observacion)
            };

            var creada = await _repositorio.CrearAccionAsync(accion);

            return MapearAccion(creada);
        }

        private async Task ValidarUsuarioAsync(long usuarioId)
        {
            if (!await _repositorio.ExisteUsuarioActivoAsync(usuarioId))
                throw new ArgumentException($"No existe un usuario activo con id {usuarioId}.");
        }

        private static DateTime ValidarFechaDeteccion(DateTime? fecha)
        {
            if (!fecha.HasValue)
                throw new ArgumentException("La fecha de detección es obligatoria.");

            if (fecha.Value.Kind != DateTimeKind.Utc)
                throw new ArgumentException("La fecha de detección debe enviarse en UTC, terminada en Z.");

            var valor = fecha.Value;

            if (valor > DateTime.UtcNow.AddMinutes(5))
                throw new ArgumentException("La fecha de detección no puede estar en el futuro.");

            return valor.AddTicks(-(valor.Ticks % TimeSpan.TicksPerSecond));
        }

        private static string Normalizar(string valor)
        {
            return valor.Trim().ToUpperInvariant();
        }

        private static string? Limpiar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private static IncidenteRespuestaDto MapearRespuesta(
            Incidente incidente)
        {
            return new IncidenteRespuestaDto
            {
                Id = incidente.InId,
                UsuarioId = incidente.UsId,
                FechaDeteccion = incidente.InFechaDete,
                Tipo = incidente.InTipo,
                Modulo = incidente.InModulo,
                Descripcion = incidente.InDescri,
                DatosAfectados = incidente.InDatosAfec,
                ImpactoPreliminar = incidente.InImpacPreli,
                Estado = incidente.InEstad,
                FechaCierre = incidente.InFechaCier,
                Version = incidente.InVersion
            };
        }

        private static AccionIncidenteRespuestaDto MapearAccion(
            AccionIncidente accion)
        {
            return new AccionIncidenteRespuestaDto
            {
                Id = accion.AcId,
                IncidenteId = accion.InId,
                UsuarioId = accion.UsId,
                FechaHora = accion.AcFechaHora,
                Tipo = accion.AcTipo,
                Accion = accion.AcAccion,
                Observacion = accion.AcObser
            };
        }
    }
}