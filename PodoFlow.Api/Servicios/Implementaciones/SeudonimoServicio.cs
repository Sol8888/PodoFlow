using PodoFlow.Api.DTOs.Seudonimo;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    public class SeudonimoServicio : ISeudonimoServicio
    {
        private const string CodigoAnalitica = "ANALITICA";
        private const string Aceptado = "ACEPTADO";

        private readonly ISeudonimoRepositorio _repositorio;
        private readonly IFinalidadRepositorio _finalidadRepositorio;

        public SeudonimoServicio(
            ISeudonimoRepositorio repositorio,
            IFinalidadRepositorio finalidadRepositorio)
        {
            _repositorio = repositorio;
            _finalidadRepositorio = finalidadRepositorio;
        }

        public async Task<List<SeudonimoRespuestaDto>> ObtenerTodosAsync(bool incluirInactivos, int pagina, int tamano)
        {
            var seudonimos = await _repositorio.ObtenerTodosAsync(incluirInactivos, pagina, tamano);

            return seudonimos.Select(MapearRespuesta).ToList();
        }

        public async Task<SeudonimoRespuestaDto?> ObtenerPorIdAsync(Guid id)
        {
            var seudonimo = await _repositorio.ObtenerPorIdAsync(id);

            return seudonimo == null ? null : MapearRespuesta(seudonimo);
        }

        public async Task<SeudonimoRespuestaDto?> ObtenerPorPacienteAsync(long pacienteId)
        {
            var seudonimo = await _repositorio.ObtenerPorPacienteAsync(pacienteId);

            return seudonimo == null ? null : MapearRespuesta(seudonimo);
        }

        public async Task<SeudonimoRespuestaDto> CrearAsync(
            SeudonimoCrearDto dto)
        {
            await ValidarPacienteAsync(dto.PacienteId);

            var existente = await _repositorio.ObtenerPorPacienteAsync(dto.PacienteId);

            if (existente != null && existente.SeEstatus)
                throw new InvalidOperationException(
                    $"El paciente {dto.PacienteId} ya tiene seudónimo activo ({existente.SeId}).");

            if (existente != null)
            {
     
                existente.SeEstatus = true;

                await _repositorio.GuardarCambiosAsync();

                return MapearRespuesta(existente);
            }

            var seudonimo = NuevoSeudonimo(dto.PacienteId);

            var creado = await _repositorio.CrearAsync(seudonimo);

            return MapearRespuesta(creado);
        }

        // Crea de una vez los seudónimos que faltan antes de una ejecución analítica.
        public async Task<SeudonimoGeneracionRespuestaDto> GenerarAsync()
        {
            var finalidad = await ObtenerFinalidadAnaliticaAsync();

            var pacientes = await _repositorio.ObtenerPacientesSinSeudonimoAsync(finalidad.FiId);

            if (pacientes.Count > 0)
                await _repositorio.CrearVariosAsync(pacientes.Select(NuevoSeudonimo).ToList());

            var conSeudonimo = await _repositorio.ContarPacientesConSeudonimoActivoAsync(finalidad.FiId);

            return new SeudonimoGeneracionRespuestaDto
            {
                Creados = pacientes.Count,
                YaExistentes = conSeudonimo - pacientes.Count
            };
        }
        public async Task<bool> EliminarAsync(Guid id)
        {
            var seudonimo = await _repositorio.ObtenerPorIdAsync(id);

            if (seudonimo == null)
                return false;

            if (!seudonimo.SeEstatus)
                return true;

            seudonimo.SeEstatus = false;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        public async Task<bool> ReactivarAsync(Guid id)
        {
            var seudonimo = await _repositorio.ObtenerPorIdAsync(id);

            if (seudonimo == null)
                return false;

            if (seudonimo.SeEstatus)
                return true;

            await ValidarPacienteAsync(seudonimo.PaId);

            seudonimo.SeEstatus = true;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        private async Task ValidarPacienteAsync(long pacienteId)
        {
            if (!await _repositorio.ExistePacienteActivoAsync(pacienteId))
                throw new ArgumentException($"No existe un paciente activo con id {pacienteId}.");

            var finalidad = await ObtenerFinalidadAnaliticaAsync();

            var estado = await _repositorio.ObtenerEstadoConsentimientoAsync(pacienteId, finalidad.FiId);

            if (estado != Aceptado)
                throw new InvalidOperationException(
                    $"El paciente {pacienteId} no tiene el consentimiento {CodigoAnalitica} aceptado" +
                    (estado == null ? "." : $" (está {estado}).") +
                    " Sin él no se puede incluir en la analítica.");
        }

        private async Task<Finalidad> ObtenerFinalidadAnaliticaAsync()
        {
            var finalidad = await _finalidadRepositorio.ObtenerPorCodigoAsync(CodigoAnalitica);

            if (finalidad == null || !finalidad.FiEstatus)
                throw new InvalidOperationException($"La finalidad {CodigoAnalitica} no existe o está dada de baja.");

            return finalidad;
        }

        private static Seudonimo NuevoSeudonimo(long pacienteId)
        {
            return new Seudonimo
            {
                SeId = Guid.NewGuid(),
                PaId = pacienteId,
                SeFechaCrea = DateTime.UtcNow,
                SeEstatus = true
            };
        }

        private static SeudonimoRespuestaDto MapearRespuesta(
            Seudonimo seudonimo)
        {
            return new SeudonimoRespuestaDto
            {
                Id = seudonimo.SeId,
                PacienteId = seudonimo.PaId,
                FechaCreacion = seudonimo.SeFechaCrea,
                Estatus = seudonimo.SeEstatus
            };
        }
    }
}
