using PodoFlow.Api.DTOs.OpcionComunicacion;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    // Caso A con UNIQUE (paciente, canal, finalidad): si ya existe dada de baja, se reactiva y actualiza.
    // Si la finalidad requiere consentimiento y el paciente da permiso, co_id debe apuntar a su
    // consentimiento ACEPTADO para esa finalidad.
    // Convención: null/false → 404 · ArgumentException → 400 · InvalidOperationException → 409.
    public class OpcionComunicacionServicio : IOpcionComunicacionServicio
    {
        private const string Aceptado = "ACEPTADO";

        // Mismos valores que CK_OPCION_canal.
        private static readonly HashSet<string> Canales = ["CORREO", "WHATSAPP"];

        private readonly IOpcionComunicacionRepositorio _repositorio;
        private readonly IFinalidadRepositorio _finalidadRepositorio;

        public OpcionComunicacionServicio(
            IOpcionComunicacionRepositorio repositorio,
            IFinalidadRepositorio finalidadRepositorio)
        {
            _repositorio = repositorio;
            _finalidadRepositorio = finalidadRepositorio;
        }

        public async Task<List<OpcionComunicacionRespuestaDto>> ObtenerTodosAsync(
            long? pacienteId, int? finalidadId, string? canal, bool incluirInactivos, int pagina, int tamano)
        {
            var filtroCanal = string.IsNullOrWhiteSpace(canal) ? null : NormalizarCanal(canal);

            var opciones = await _repositorio.ObtenerTodosAsync(
                pacienteId, finalidadId, filtroCanal, incluirInactivos, pagina, tamano);

            return opciones.Select(MapearRespuesta).ToList();
        }

        public async Task<OpcionComunicacionRespuestaDto?> ObtenerPorIdAsync(long id)
        {
            var opcion = await _repositorio.ObtenerPorIdAsync(id);

            return opcion == null ? null : MapearRespuesta(opcion);
        }

        public async Task<OpcionComunicacionRespuestaDto> CrearAsync(
            OpcionComunicacionCrearDto dto)
        {
            var canal = NormalizarCanal(dto.Canal);
            var permitido = dto.Permitido!.Value;

            var finalidad = await ValidarPacienteYFinalidadAsync(dto.PacienteId, dto.FinalidadId);

            var consentimientoId = await ResolverConsentimientoAsync(
                dto.PacienteId, finalidad, permitido);

            var existente = await _repositorio.ObtenerPorClaveAsync(dto.PacienteId, canal, dto.FinalidadId);

            if (existente != null)
            {
                if (existente.OpEstatus)
                    throw new InvalidOperationException(
                        $"El paciente ya tiene una opción para {finalidad.FiCodigo} por {canal} (id {existente.OpId}). " +
                        $"Use PUT api/OpcionComunicacion/{existente.OpId} para cambiar el permiso.");

                // Estaba dada de baja: se reactiva y actualiza en lugar de insertar (la clave sigue ocupada).
                existente.OpPermi = permitido;
                existente.CoId = consentimientoId;
                existente.OpFechaActu = DateTime.UtcNow;
                existente.OpEstatus = true;

                await _repositorio.GuardarCambiosAsync();

                return MapearRespuesta(existente);
            }

            var opcion = new OpcionComunicacion
            {
                PaId = dto.PacienteId,
                FiId = dto.FinalidadId,
                CoId = consentimientoId,
                OpCanal = canal,
                OpPermi = permitido,
                OpFechaActu = DateTime.UtcNow,
                OpEstatus = true
            };

            var creada = await _repositorio.CrearAsync(opcion);

            return MapearRespuesta(creada);
        }

        public async Task<bool> ActualizarAsync(
            long id,
            OpcionComunicacionActualizarDto dto)
        {
            var opcion = await _repositorio.ObtenerPorIdAsync(id);

            if (opcion == null)
                return false;

            var permitido = dto.Permitido!.Value;

            // Quitar el permiso siempre se puede (es lo que protege al paciente).
            // Darlo exige paciente y finalidad activos y, si corresponde, consentimiento aceptado.
            if (permitido)
            {
                var finalidad = await ValidarPacienteYFinalidadAsync(opcion.PaId, opcion.FiId);

                opcion.CoId = await ResolverConsentimientoAsync(opcion.PaId, finalidad, permitido);
            }
            else
            {
                opcion.CoId = null;
            }

            // Nunca se tocan OpId, PaId, FiId, OpCanal ni OpEstatus.
            opcion.OpPermi = permitido;
            opcion.OpFechaActu = DateTime.UtcNow;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        public async Task<bool> EliminarAsync(long id)
        {
            var opcion = await _repositorio.ObtenerPorIdAsync(id);

            if (opcion == null)
                return false;

            // Si ya estaba dada de baja no hay nada que hacer.
            if (!opcion.OpEstatus)
                return true;

            opcion.OpEstatus = false;
            opcion.OpFechaActu = DateTime.UtcNow;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        public async Task<bool> ReactivarAsync(long id)
        {
            var opcion = await _repositorio.ObtenerPorIdAsync(id);

            if (opcion == null)
                return false;

            // Si ya estaba activa no hay nada que hacer.
            if (opcion.OpEstatus)
                return true;

            // Mientras estuvo de baja pudo cambiar el paciente, la finalidad o el consentimiento.
            var finalidad = await ValidarPacienteYFinalidadAsync(opcion.PaId, opcion.FiId);

            opcion.CoId = await ResolverConsentimientoAsync(opcion.PaId, finalidad, opcion.OpPermi);
            opcion.OpEstatus = true;
            opcion.OpFechaActu = DateTime.UtcNow;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        private async Task<Finalidad> ValidarPacienteYFinalidadAsync(long pacienteId, int finalidadId)
        {
            if (!await _repositorio.ExistePacienteActivoAsync(pacienteId))
                throw new ArgumentException($"No existe un paciente activo con id {pacienteId}.");

            var finalidad = await _finalidadRepositorio.ObtenerPorIdAsync(finalidadId);

            if (finalidad == null || !finalidad.FiEstatus)
                throw new ArgumentException($"No existe una finalidad activa con id {finalidadId}.");

            return finalidad;
        }

        // Devuelve el co_id que respalda el permiso, o null si no hace falta.
        // Si la finalidad requiere consentimiento (fi_requi_cons) y el paciente da permiso,
        // tiene que existir su consentimiento ACEPTADO para esa finalidad.
        private async Task<long?> ResolverConsentimientoAsync(long pacienteId, Finalidad finalidad, bool permitido)
        {
            // Sin permiso no se enviará nada, así que no hay consentimiento que enlazar.
            if (!permitido)
                return null;

            var consentimiento = await _repositorio.ObtenerConsentimientoAsync(pacienteId, finalidad.FiId);

            if (consentimiento != null && consentimiento.CoEstad == Aceptado)
                return consentimiento.CoId;

            if (!finalidad.FiRequiCons)
                return null;

            if (consentimiento == null)
                throw new InvalidOperationException(
                    $"La finalidad {finalidad.FiCodigo} requiere consentimiento y el paciente no lo tiene registrado. " +
                    "Regístrelo primero en api/Consentimiento.");

            throw new InvalidOperationException(
                $"La finalidad {finalidad.FiCodigo} requiere consentimiento y el del paciente " +
                $"(id {consentimiento.CoId}) está {consentimiento.CoEstad}.");
        }

        private static string NormalizarCanal(string canal)
        {
            var normalizado = canal.Trim().ToUpperInvariant();

            if (!Canales.Contains(normalizado))
                throw new ArgumentException("El canal debe ser CORREO o WHATSAPP.");

            return normalizado;
        }

        private static OpcionComunicacionRespuestaDto MapearRespuesta(
            OpcionComunicacion opcion)
        {
            return new OpcionComunicacionRespuestaDto
            {
                Id = opcion.OpId,
                PacienteId = opcion.PaId,
                FinalidadId = opcion.FiId,
                ConsentimientoId = opcion.CoId,
                Canal = opcion.OpCanal,
                Permitido = opcion.OpPermi,
                FechaActualizacion = opcion.OpFechaActu,
                Estatus = opcion.OpEstatus
            };
        }
    }
}
