using PodoFlow.Api.DTOs.TratamientoCategoria;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    // Caso A en tabla intermedia. La PK (tr_id, ca_id) funciona como el UNIQUE:
    // una misma pareja no se puede insertar dos veces, así que se reactiva.
    // Convención: null/false → 404 · ArgumentException → 400 · InvalidOperationException → 409.
    public class TratamientoCategoriaServicio : ITratamientoCategoriaServicio
    {
        private readonly ITratamientoCategoriaRepositorio _repositorio;
        private readonly ITratamientoDatosRepositorio _tratamientoRepositorio;
        private readonly ICategoriaDatoRepositorio _categoriaRepositorio;

        public TratamientoCategoriaServicio(
            ITratamientoCategoriaRepositorio repositorio,
            ITratamientoDatosRepositorio tratamientoRepositorio,
            ICategoriaDatoRepositorio categoriaRepositorio)
        {
            _repositorio = repositorio;
            _tratamientoRepositorio = tratamientoRepositorio;
            _categoriaRepositorio = categoriaRepositorio;
        }

        public async Task<List<TratamientoCategoriaRespuestaDto>> ObtenerTodosAsync(
            int? tratamientoId, int? categoriaId, bool incluirInactivos, int pagina, int tamano)
        {
            var asignaciones = await _repositorio.ObtenerTodosAsync(
                tratamientoId, categoriaId, incluirInactivos, pagina, tamano);

            return asignaciones.Select(MapearRespuesta).ToList();
        }

        public async Task<TratamientoCategoriaRespuestaDto?> ObtenerAsync(int tratamientoId, int categoriaId)
        {
            var asignacion = await _repositorio.ObtenerAsync(tratamientoId, categoriaId);

            return asignacion == null ? null : MapearRespuesta(asignacion);
        }

        public async Task<TratamientoCategoriaRespuestaDto> CrearAsync(
            TratamientoCategoriaCrearDto dto)
        {
            await ValidarReferenciasAsync(dto.TratamientoId, dto.CategoriaId);

            var existente = await _repositorio.ObtenerAsync(dto.TratamientoId, dto.CategoriaId);

            if (existente != null)
            {
                if (existente.TcEstatus)
                    throw new InvalidOperationException(
                        "La categoría ya está asignada a este tratamiento.");

                // Estaba dada de baja: se reactiva. Es una asignación nueva, así que la fecha se actualiza.
                existente.TcEstatus = true;
                existente.TcFechaAsig = DateTime.UtcNow;

                await _repositorio.GuardarCambiosAsync();

                return MapearRespuesta(existente);
            }

            var asignacion = new TratamientoCategoria
            {
                TrId = dto.TratamientoId,
                CaId = dto.CategoriaId,
                TcFechaAsig = DateTime.UtcNow,
                TcEstatus = true
            };

            var creada = await _repositorio.CrearAsync(asignacion);

            return MapearRespuesta(creada);
        }

        public async Task<bool> EliminarAsync(int tratamientoId, int categoriaId)
        {
            var asignacion = await _repositorio.ObtenerAsync(tratamientoId, categoriaId);

            if (asignacion == null)
                return false;

            // Si ya estaba dada de baja no hay nada que hacer.
            if (!asignacion.TcEstatus)
                return true;

            asignacion.TcEstatus = false;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        public async Task<bool> ReactivarAsync(int tratamientoId, int categoriaId)
        {
            var asignacion = await _repositorio.ObtenerAsync(tratamientoId, categoriaId);

            if (asignacion == null)
                return false;

            // Si ya estaba activa no hay nada que hacer.
            if (asignacion.TcEstatus)
                return true;

            // No se reactiva una asignación cuyo tratamiento o categoría ya no estén activos.
            await ValidarReferenciasAsync(tratamientoId, categoriaId);

            asignacion.TcEstatus = true;
            asignacion.TcFechaAsig = DateTime.UtcNow;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        private async Task ValidarReferenciasAsync(int tratamientoId, int categoriaId)
        {
            var tratamiento = await _tratamientoRepositorio.ObtenerPorIdAsync(tratamientoId);

            if (tratamiento == null || !tratamiento.TrEstatus)
                throw new ArgumentException($"No existe un tratamiento activo con id {tratamientoId}.");

            var categoria = await _categoriaRepositorio.ObtenerPorIdAsync(categoriaId);

            if (categoria == null || !categoria.CaEstatus)
                throw new ArgumentException($"No existe una categoría de dato activa con id {categoriaId}.");
        }

        private static TratamientoCategoriaRespuestaDto MapearRespuesta(
            TratamientoCategoria asignacion)
        {
            return new TratamientoCategoriaRespuestaDto
            {
                TratamientoId = asignacion.TrId,
                CategoriaId = asignacion.CaId,
                FechaAsignacion = asignacion.TcFechaAsig,
                Estatus = asignacion.TcEstatus
            };
        }
    }
}
