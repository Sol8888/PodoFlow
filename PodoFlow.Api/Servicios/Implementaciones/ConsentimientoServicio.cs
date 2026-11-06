using System.Text.RegularExpressions;
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

        private const int MayoriaDeEdad = 18;
        private const int HorasEcuadorUtc = -5;
        private const int LetrasMinimasNombre = 3;
        private const string AutorizaTitular = "Autorizado por el titular";
        private const string AutorizaRepresentante = "Autorizado por representante: ";

        private static readonly Regex NombreValido = new(@"^\p{L}[\p{L} .'-]*$", RegexOptions.Compiled);

        private static readonly HashSet<string> MediosPermitidos = ["PRESENCIAL", "CORREO", "WHATSAPP", "TELEFONO"];

        private static readonly HashSet<string> RelacionesPermitidas = new(StringComparer.OrdinalIgnoreCase)
        {
            "Madre o padre",
            "Tutor o curador",
            "Cónyuge o pareja",
            "Hijo o hija",
            "Apoderado",
            "Otro"
        };

        private static readonly Dictionary<string, string[]> TransicionesPermitidas = new()
        {
            [Aceptado] = [Revocado],
            [Rechazado] = [Aceptado],
            [Revocado] = [Aceptado]
        };

        private readonly IConsentimientoRepositorio _repositorio;
        private readonly IFinalidadRepositorio _finalidadRepositorio;
        private readonly IAvisoRepositorio _avisoRepositorio;
        private readonly IPacienteRepositorio _pacienteRepositorio;

        public ConsentimientoServicio(
            IConsentimientoRepositorio repositorio,
            IFinalidadRepositorio finalidadRepositorio,
            IAvisoRepositorio avisoRepositorio,
            IPacienteRepositorio pacienteRepositorio)
        {
            _repositorio = repositorio;
            _finalidadRepositorio = finalidadRepositorio;
            _avisoRepositorio = avisoRepositorio;
            _pacienteRepositorio = pacienteRepositorio;
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
            var medio = NormalizarMedio(dto.Medio);

            if (estado != Aceptado && estado != Rechazado)
                throw new ArgumentException("Un consentimiento nuevo solo puede registrarse como ACEPTADO o RECHAZADO.");

            var paciente = await _pacienteRepositorio.ObtenerPorIdAsync(dto.PacienteId);

            if (paciente == null || !paciente.PaEstatus)
                throw new ArgumentException($"No existe un paciente activo con id {dto.PacienteId}.");

            await ValidarFinalidadConConsentimientoAsync(dto.FinalidadId);
            await ValidarAvisoVigenteAsync(dto.AvisoId);
            await ValidarUsuarioAsync(dto.UsuarioId);

            var observacion = ComponerObservacion(paciente, dto.RepresentanteNombre, dto.RepresentanteRelacion, dto.Observacion);

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
                CoMedio = medio,
                CoFechaObte = ahora,
                CoObser = observacion
            };

            var evento = CrearEvento(estado, medio, dto.RutaEvidencia, observacion, dto.UsuarioId, ahora);

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
            var medio = NormalizarMedio(dto.Medio);

            var consentimiento = await _repositorio.ObtenerPorIdAsync(id);

            if (consentimiento == null)
                return null;

            if (!TransicionesPermitidas[consentimiento.CoEstad].Contains(nuevoEstado))
                throw new InvalidOperationException(
                    $"No se puede pasar de {consentimiento.CoEstad} a {nuevoEstado}.");

            await ValidarUsuarioAsync(dto.UsuarioId);

            var paciente = await _pacienteRepositorio.ObtenerPorIdAsync(consentimiento.PaId)
                ?? throw new InvalidOperationException($"No existe el paciente {consentimiento.PaId} del consentimiento.");

            if (nuevoEstado == Aceptado && !paciente.PaEstatus)
                throw new InvalidOperationException("El paciente está dado de baja: no se puede aceptar un consentimiento.");

            var observacion = ComponerObservacion(paciente, dto.RepresentanteNombre, dto.RepresentanteRelacion, dto.Observacion);
            var ahora = DateTime.UtcNow;

            if (nuevoEstado == Aceptado)
            {
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
            consentimiento.CoMedio = medio;
            consentimiento.CoObser = observacion;

            var evento = CrearEvento(nuevoEstado, medio, dto.RutaEvidencia, observacion, dto.UsuarioId, ahora);
            evento.CoId = consentimiento.CoId;

            await _repositorio.CambiarEstadoAsync(consentimiento, evento, dto.Version);

            return MapearRespuesta(consentimiento);
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

        private async Task ValidarUsuarioAsync(long? usuarioId)
        {
            if (usuarioId.HasValue && !await _repositorio.ExisteUsuarioActivoAsync(usuarioId.Value))
                throw new ArgumentException($"No existe un usuario activo con id {usuarioId.Value}.");
        }

        private async Task ValidarFinalidadConConsentimientoAsync(int finalidadId)
        {
            var finalidad = await ValidarFinalidadAsync(finalidadId);

            if (!finalidad.FiRequiCons)
                throw new ArgumentException(
                    $"La finalidad '{finalidad.FiNombre}' no requiere consentimiento: no se registra uno para ella.");
        }

        private async Task<Finalidad> ValidarFinalidadAsync(int finalidadId)
        {
            var finalidad = await _finalidadRepositorio.ObtenerPorIdAsync(finalidadId);

            if (finalidad == null || !finalidad.FiEstatus)
                throw new ArgumentException($"No existe una finalidad activa con id {finalidadId}.");

            return finalidad;
        }

        private async Task ValidarAvisoVigenteAsync(int avisoId)
        {
            var aviso = await _avisoRepositorio.ObtenerPorIdAsync(avisoId);
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(HorasEcuadorUtc));

            if (aviso == null || !aviso.AvEstatus)
                throw new ArgumentException($"No existe un aviso activo con id {avisoId}.");

            if (aviso.AvFechaDesd > hoy || (aviso.AvFechaHast.HasValue && aviso.AvFechaHast.Value < hoy))
                throw new ArgumentException($"El aviso '{aviso.AvVersi}' no está vigente.");
        }

        private static string ComponerObservacion(Paciente paciente, string? nombre, string? relacion, string? nota)
        {
            var autoriza = AutorizaTitular;

            if (string.IsNullOrWhiteSpace(nombre) && string.IsNullOrWhiteSpace(relacion))
            {
                if (EsMenorDeEdad(paciente))
                    throw new ArgumentException("El paciente es menor de edad: su representante debe autorizar esta decisión.");
            }
            else
            {
                var nombreLimpio = string.Join(' ', (nombre ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries));

                if (!NombreValido.IsMatch(nombreLimpio) || nombreLimpio.Count(char.IsLetter) < LetrasMinimasNombre)
                    throw new ArgumentException("El nombre del representante debe tener al menos 3 letras y no llevar números.");

                if (!RelacionesPermitidas.TryGetValue((relacion ?? string.Empty).Trim(), out var relacionValida))
                    throw new ArgumentException(
                        $"La relación del representante debe ser una de: {string.Join(", ", RelacionesPermitidas)}.");

                autoriza = $"{AutorizaRepresentante}{nombreLimpio} ({relacionValida})";
            }

            return string.IsNullOrWhiteSpace(nota) ? autoriza : $"{autoriza}. {nota.Trim()}";
        }

        private static bool EsMenorDeEdad(Paciente paciente)
        {
            if (paciente.PaFechaNaci == null)
                return false;

            var hoy = DateTime.UtcNow.AddHours(HorasEcuadorUtc).Date;

            return paciente.PaFechaNaci.Value.Date.AddYears(MayoriaDeEdad) > hoy;
        }

        private static string NormalizarMedio(string medio)
        {
            var normalizado = medio.Trim().ToUpperInvariant();

            if (!MediosPermitidos.Contains(normalizado))
                throw new ArgumentException("El medio debe ser PRESENCIAL, CORREO, WHATSAPP o TELEFONO.");

            return normalizado;
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
                EvMedio = medio,
                EvRutaEvid = rutaEvidencia?.Trim(),
                EvObser = observacion
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
    }
}
