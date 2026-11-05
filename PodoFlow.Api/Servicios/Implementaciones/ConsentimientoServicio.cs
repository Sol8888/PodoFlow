using PodoFlow.Api.DTOs.Consentimiento;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    public class ConsentimientoServicio : IConsentimientoServicio
    {
        private const string Aceptado = "ACEPTADO";
        private const string Rechazado = "RECHAZADO";
        private const string Revocado = "REVOCADO";

        private static readonly Dictionary<string, string[]> TransicionesPermitidas = new()
        {
            [Aceptado] = [Revocado],            
            [Rechazado] = [Aceptado],           
            [Revocado] = [Aceptado]             
        };

        private readonly IConsentimientoRepositorio _repositorio;
        private readonly IFinalidadRepositorio _finalidadRepositorio;
        private readonly IAvisoRepositorio _avisoRepositorio;

        public ConsentimientoServicio(
            IConsentimientoRepositorio repositorio,
            IFinalidadRepositorio finalidadRepositorio,
            IAvisoRepositorio avisoRepositorio)
        {
            _repositorio = repositorio;
            _finalidadRepositorio = finalidadRepositorio;
            _avisoRepositorio = avisoRepositorio;
        }

        public async Task<List<ConsentimientoRespuestaDto>> ObtenerTodosAsync(int pagina, int tamano)
        {
            var consentimientos = await _repositorio.ObtenerTodosAsync(pagina, tamano);

            return consentimientos.Select(MapearRespuesta).ToList();
        }

        public async Task<List<ConsentimientoRespuestaDto>> ObtenerPorPacienteAsync(long pacienteId)
        {
            var consentimientos = await _repositorio.ObtenerPorPacienteAsync(pacienteId);

            return consentimientos.Select(MapearRespuesta).ToList();
        }

        public async Task<ConsentimientoRespuestaDto?> ObtenerPorIdAsync(long id)
        {
            var consentimiento = await _repositorio.ObtenerPorIdAsync(id);

            if (consentimiento == null)
                return null;

            return MapearRespuesta(consentimiento);
        }

        public async Task<ConsentimientoRespuestaDto> CrearAsync(
            ConsentimientoCrearDto dto)
        {
            var estado = NormalizarEstado(dto.Estado);

            if (estado != Aceptado && estado != Rechazado)
                throw new ArgumentException("Un consentimiento nuevo solo puede registrarse como ACEPTADO o RECHAZADO.");

            if (!await _repositorio.ExistePacienteActivoAsync(dto.PacienteId))
                throw new ArgumentException($"No existe un paciente activo con id {dto.PacienteId}.");

            await ValidarFinalidadAsync(dto.FinalidadId);
            await ValidarAvisoVigenteAsync(dto.AvisoId);
            await ValidarUsuarioAsync(dto.UsuarioId);

            // UQ_CONSENTIMIENTO_pac_fin. Aquí no hay estatus que reactivar:
            // si ya existe, lo que corresponde es cambiarle el estado.
            if (await _repositorio.ExistePorPacienteYFinalidadAsync(dto.PacienteId, dto.FinalidadId))
                throw new InvalidOperationException(
                    "El paciente ya tiene un consentimiento para esa finalidad. Use el cambio de estado.");

            var ahora = DateTime.UtcNow;

            var consentimiento = new Consentimiento
            {
                PaId = dto.PacienteId,
                FiId = dto.FinalidadId,
                AvId = dto.AvisoId,
                CoEstad = estado,
                CoMedio = dto.Medio.Trim(),
                CoFechaObte = ahora,
                CoObser = dto.Observacion?.Trim()
            };

            var evento = CrearEvento(estado, dto.Medio, dto.RutaEvidencia, dto.Observacion, dto.UsuarioId, ahora);

            var creado = await _repositorio.CrearAsync(consentimiento, evento);

            return MapearRespuesta(creado);
        }

        public async Task<bool> ActualizarAsync(
            long id,
            ConsentimientoActualizarDto dto)
        {
            var consentimiento = await _repositorio.ObtenerPorIdAsync(id);

            if (consentimiento == null)
                return false;

            consentimiento.CoMedio = dto.Medio.Trim();
            consentimiento.CoObser = dto.Observacion?.Trim();

            await _repositorio.ActualizarAsync(consentimiento, dto.Version);

            return true;
        }

        public async Task<ConsentimientoRespuestaDto?> CambiarEstadoAsync(
            long id,
            ConsentimientoCambiarEstadoDto dto)
        {
            var nuevoEstado = NormalizarEstado(dto.Estado);

            var consentimiento = await _repositorio.ObtenerPorIdAsync(id);

            if (consentimiento == null)
                return null;

            if (!TransicionesPermitidas[consentimiento.CoEstad].Contains(nuevoEstado))
                throw new InvalidOperationException(
                    $"No se puede pasar de {consentimiento.CoEstad} a {nuevoEstado}.");

            await ValidarUsuarioAsync(dto.UsuarioId);

            var ahora = DateTime.UtcNow;

            if (nuevoEstado == Aceptado)
            {
                // Al (re)aceptar, el paciente acepta la versión vigente del aviso.
                if (dto.AvisoId == null)
                    throw new ArgumentException("Para aceptar hay que indicar la versión del aviso (AvisoId).");

                await ValidarAvisoVigenteAsync(dto.AvisoId.Value);
                await ValidarFinalidadAsync(consentimiento.FiId);

                consentimiento.AvId = dto.AvisoId.Value;
                consentimiento.CoFechaObte = ahora;
                consentimiento.CoFechaRevo = null;
            }
            else if (nuevoEstado == Revocado)
            {
                consentimiento.CoFechaRevo = ahora;
            }

            consentimiento.CoEstad = nuevoEstado;
            consentimiento.CoMedio = dto.Medio.Trim();

            var evento = CrearEvento(nuevoEstado, dto.Medio, dto.RutaEvidencia, dto.Observacion, dto.UsuarioId, ahora);
            evento.CoId = consentimiento.CoId;

            await _repositorio.CambiarEstadoAsync(consentimiento, evento, dto.Version);

            return MapearRespuesta(consentimiento);
        }

        private async Task ValidarUsuarioAsync(long? usuarioId)
        {
            if (usuarioId.HasValue && !await _repositorio.ExisteUsuarioActivoAsync(usuarioId.Value))
                throw new ArgumentException($"No existe un usuario activo con id {usuarioId.Value}.");
        }

        private async Task ValidarFinalidadAsync(int finalidadId)
        {
            var finalidad = await _finalidadRepositorio.ObtenerPorIdAsync(finalidadId);

            if (finalidad == null || !finalidad.FiEstatus)
                throw new ArgumentException($"No existe una finalidad activa con id {finalidadId}.");
        }

        private async Task ValidarAvisoVigenteAsync(int avisoId)
        {
            var aviso = await _avisoRepositorio.ObtenerPorIdAsync(avisoId);
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

            if (aviso == null || !aviso.AvEstatus)
                throw new ArgumentException($"No existe un aviso activo con id {avisoId}.");

            if (aviso.AvFechaDesd > hoy || (aviso.AvFechaHast.HasValue && aviso.AvFechaHast.Value < hoy))
                throw new ArgumentException($"El aviso '{aviso.AvVersi}' no está vigente.");
        }

        private static string NormalizarEstado(string estado)
        {
            var normalizado = estado.Trim().ToUpperInvariant();

            if (normalizado != Aceptado && normalizado != Rechazado && normalizado != Revocado)
                throw new ArgumentException("El estado debe ser ACEPTADO, RECHAZADO o REVOCADO.");

            return normalizado;
        }

        private static EventoConsentimiento CrearEvento(
            string estado, string medio, string? rutaEvidencia, string? observacion, long? usuarioId, DateTime fecha)
        {
            return new EventoConsentimiento
            {
                UsId = usuarioId,
                EvEstad = estado,
                EvFechaHora = fecha,
                EvMedio = medio.Trim(),
                EvRutaEvid = rutaEvidencia?.Trim(),
                EvObser = observacion?.Trim()
            };
        }

        private static ConsentimientoRespuestaDto MapearRespuesta(
            Consentimiento consentimiento)
        {
            return new ConsentimientoRespuestaDto
            {
                Id = consentimiento.CoId,
                PacienteId = consentimiento.PaId,
                FinalidadId = consentimiento.FiId,
                AvisoId = consentimiento.AvId,
                Estado = consentimiento.CoEstad,
                Medio = consentimiento.CoMedio,
                FechaObtencion = consentimiento.CoFechaObte,
                FechaRevocacion = consentimiento.CoFechaRevo,
                Observacion = consentimiento.CoObser,
                Version = consentimiento.CoVersion
            };
        }
        public async Task<List<EventoConsentimientoRespuestaDto>?> ObtenerEventosAsync(long id)
        {
            var consentimiento = await _repositorio.ObtenerPorIdAsync(id);

            if (consentimiento == null)
                return null;

            var eventos = await _repositorio.ObtenerEventosAsync(id);

            return eventos.Select(e => new EventoConsentimientoRespuestaDto
            {
                Id = e.EvId,
                ConsentimientoId = e.CoId,
                UsuarioId = e.UsId,
                Estado = e.EvEstad,
                FechaHora = e.EvFechaHora,
                Medio = e.EvMedio,
                RutaEvidencia = e.EvRutaEvid,
                Observacion = e.EvObser
            }).ToList();
        }
    }
}