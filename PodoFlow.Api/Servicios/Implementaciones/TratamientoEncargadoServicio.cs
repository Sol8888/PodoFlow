using PodoFlow.Api.DTOs.TratamientoEncargado;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    // Caso A en tabla intermedia. La PK (tr_id, en_id) funciona como el UNIQUE:
    // una misma pareja no se puede insertar dos veces, así que se reactiva.
    // Convención: null/false → 404 · ArgumentException → 400 · InvalidOperationException → 409.
    public class TratamientoEncargadoServicio : ITratamientoEncargadoServicio
    {
        private readonly ITratamientoEncargadoRepositorio _repositorio;
        private readonly ITratamientoDatosRepositorio _tratamientoRepositorio;
        private readonly IEncargadoRepositorio _encargadoRepositorio;

        public TratamientoEncargadoServicio(
            ITratamientoEncargadoRepositorio repositorio,
            ITratamientoDatosRepositorio tratamientoRepositorio,
            IEncargadoRepositorio encargadoRepositorio)
        {
            _repositorio = repositorio;
            _tratamientoRepositorio = tratamientoRepositorio;
            _encargadoRepositorio = encargadoRepositorio;
        }

        public async Task<List<TratamientoEncargadoRespuestaDto>> ObtenerTodosAsync(
            int? tratamientoId, int? encargadoId, bool incluirInactivos, int pagina, int tamano)
        {
            var asignaciones = await _repositorio.ObtenerTodosAsync(
                tratamientoId, encargadoId, incluirInactivos, pagina, tamano);

            return asignaciones.Select(MapearRespuesta).ToList();
        }

        public async Task<TratamientoEncargadoRespuestaDto?> ObtenerAsync(int tratamientoId, int encargadoId)
        {
            var asignacion = await _repositorio.ObtenerAsync(tratamientoId, encargadoId);

            return asignacion == null ? null : MapearRespuesta(asignacion);
        }

        public async Task<TratamientoEncargadoRespuestaDto> CrearAsync(
            TratamientoEncargadoCrearDto dto)
        {
            await ValidarReferenciasAsync(dto.TratamientoId, dto.EncargadoId);

            var existente = await _repositorio.ObtenerAsync(dto.TratamientoId, dto.EncargadoId);

            if (existente != null)
            {
                if (existente.TeEstatus)
                    throw new InvalidOperationException(
                        "El encargado ya está asignado a este tratamiento.");

                // Estaba dada de baja: se reactiva. Es una asignación nueva, así que la fecha se actualiza.
                existente.TeEstatus = true;
                existente.TeFechaAsig = DateTime.UtcNow;

                await _repositorio.GuardarCambiosAsync();

                return MapearRespuesta(existente);
            }

            var asignacion = new TratamientoEncargado
            {
                TrId = dto.TratamientoId,
                EnId = dto.EncargadoId,
                TeFechaAsig = DateTime.UtcNow,
                TeEstatus = true
            };

            var creada = await _repositorio.CrearAsync(asignacion);

            return MapearRespuesta(creada);
        }

        public async Task<bool> EliminarAsync(int tratamientoId, int encargadoId)
        {
            var asignacion = await _repositorio.ObtenerAsync(tratamientoId, encargadoId);

            if (asignacion == null)
                return false;

            // Si ya estaba dada de baja no hay nada que hacer.
            if (!asignacion.TeEstatus)
                return true;

            asignacion.TeEstatus = false;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        public async Task<bool> ReactivarAsync(int tratamientoId, int encargadoId)
        {
            var asignacion = await _repositorio.ObtenerAsync(tratamientoId, encargadoId);

            if (asignacion == null)
                return false;

            // Si ya estaba activa no hay nada que hacer.
            if (asignacion.TeEstatus)
                return true;

            // No se reactiva una asignación cuyo tratamiento o encargado ya no estén activos.
            await ValidarReferenciasAsync(tratamientoId, encargadoId);

            asignacion.TeEstatus = true;
            asignacion.TeFechaAsig = DateTime.UtcNow;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        private async Task ValidarReferenciasAsync(int tratamientoId, int encargadoId)
        {
            var tratamiento = await _tratamientoRepositorio.ObtenerPorIdAsync(tratamientoId);

            if (tratamiento == null || !tratamiento.TrEstatus)
                throw new ArgumentException($"No existe un tratamiento activo con id {tratamientoId}.");

            var encargado = await _encargadoRepositorio.ObtenerPorIdAsync(encargadoId);

            if (encargado == null || !encargado.EnEstatus)
                throw new ArgumentException($"No existe un encargado activo con id {encargadoId}.");
        }

        private static TratamientoEncargadoRespuestaDto MapearRespuesta(
            TratamientoEncargado asignacion)
        {
            return new TratamientoEncargadoRespuestaDto
            {
                TratamientoId = asignacion.TrId,
                EncargadoId = asignacion.EnId,
                FechaAsignacion = asignacion.TeFechaAsig,
                Estatus = asignacion.TeEstatus
            };
        }
    }
}
