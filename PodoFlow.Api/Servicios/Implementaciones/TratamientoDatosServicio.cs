using PodoFlow.Api.DTOs.TratamientoDatos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    public class TratamientoDatosServicio : ITratamientoDatosServicio
    {
        private readonly ITratamientoDatosRepositorio _repositorio;
        private readonly IFinalidadRepositorio _finalidadRepositorio;

        public TratamientoDatosServicio(
            ITratamientoDatosRepositorio repositorio,
            IFinalidadRepositorio finalidadRepositorio)
        {
            _repositorio = repositorio;
            _finalidadRepositorio = finalidadRepositorio;
        }

        public async Task<List<TratamientoDatosRespuestaDto>> ObtenerTodosAsync(
            bool incluirInactivos, int pagina, int tamano)
        {
            var tratamientos = await _repositorio.ObtenerTodosAsync(incluirInactivos, pagina, tamano);

            return tratamientos.Select(MapearRespuesta).ToList();
        }

        public async Task<TratamientoDatosRespuestaDto?> ObtenerPorIdAsync(int id)
        {
            var tratamiento = await _repositorio.ObtenerPorIdAsync(id);

            return tratamiento == null ? null : MapearRespuesta(tratamiento);
        }

        public async Task<TratamientoDatosRespuestaDto> CrearAsync(
            TratamientoDatosCrearDto dto)
        {
            await ValidarReferenciasAsync(dto.FinalidadId, dto.UsuarioId);

            var codigo = dto.Codigo.Trim().ToUpperInvariant();

            var existente = await _repositorio.ObtenerPorCodigoAsync(codigo);

            if (existente != null)
            {
                if (existente.TrEstatus)
                    throw new InvalidOperationException(
                        $"Ya existe un tratamiento activo con el código '{codigo}'.");

                AplicarDatos(existente, dto.FinalidadId, dto.UsuarioId, dto.Nombre, dto.Modulo,
                    dto.Descripcion, dto.CategoriasTitulares, dto.BaseLegitimacion,
                    dto.PlazoConservacion, dto.MedidasSeguridad, dto.FechaRevision);

                existente.TrEstatus = true;

                await _repositorio.GuardarCambiosAsync();

                return MapearRespuesta(existente);
            }

            var tratamiento = new TratamientoDatos
            {
                TrCodigo = codigo,
                TrEstatus = true
            };

            AplicarDatos(tratamiento, dto.FinalidadId, dto.UsuarioId, dto.Nombre, dto.Modulo,
                dto.Descripcion, dto.CategoriasTitulares, dto.BaseLegitimacion,
                dto.PlazoConservacion, dto.MedidasSeguridad, dto.FechaRevision);

            var creado = await _repositorio.CrearAsync(tratamiento);

            return MapearRespuesta(creado);
        }

        public async Task<bool> ActualizarAsync(
            int id,
            TratamientoDatosActualizarDto dto)
        {
            var tratamiento = await _repositorio.ObtenerPorIdAsync(id);

            if (tratamiento == null)
                return false;

            await ValidarReferenciasAsync(dto.FinalidadId, dto.UsuarioId);

            AplicarDatos(tratamiento, dto.FinalidadId, dto.UsuarioId, dto.Nombre, dto.Modulo,
                dto.Descripcion, dto.CategoriasTitulares, dto.BaseLegitimacion,
                dto.PlazoConservacion, dto.MedidasSeguridad, dto.FechaRevision);

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var tratamiento = await _repositorio.ObtenerPorIdAsync(id);

            if (tratamiento == null)
                return false;

            if (!tratamiento.TrEstatus)
                return true;

            tratamiento.TrEstatus = false;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        public async Task<bool> ReactivarAsync(int id)
        {
            var tratamiento = await _repositorio.ObtenerPorIdAsync(id);

            if (tratamiento == null)
                return false;

            if (tratamiento.TrEstatus)
                return true;

            tratamiento.TrEstatus = true;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        private async Task ValidarReferenciasAsync(int finalidadId, long? usuarioId)
        {
            var finalidad = await _finalidadRepositorio.ObtenerPorIdAsync(finalidadId);

            if (finalidad == null || !finalidad.FiEstatus)
                throw new ArgumentException($"No existe una finalidad activa con id {finalidadId}.");

            if (usuarioId.HasValue && !await _repositorio.ExisteUsuarioActivoAsync(usuarioId.Value))
                throw new ArgumentException($"No existe un usuario activo con id {usuarioId.Value}.");
        }

        private static void AplicarDatos(
            TratamientoDatos t, int finalidadId, long? usuarioId, string nombre, string modulo,
            string? descripcion, string? categoriasTitulares, string? baseLegitimacion,
            string? plazoConservacion, string? medidasSeguridad, DateOnly? fechaRevision)
        {
            t.FiId = finalidadId;
            t.UsId = usuarioId;
            t.TrNombre = nombre.Trim();
            t.TrModulo = modulo.Trim().ToUpperInvariant();
            t.TrDescri = Limpiar(descripcion);
            t.TrCategTitu = Limpiar(categoriasTitulares);
            t.TrBaseLegit = Limpiar(baseLegitimacion);
            t.TrPlazoConse = Limpiar(plazoConservacion);
            t.TrMediSegur = Limpiar(medidasSeguridad);
            t.TrFechaRevi = fechaRevision;
        }
        private static string? Limpiar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private static TratamientoDatosRespuestaDto MapearRespuesta(
            TratamientoDatos t)
        {
            return new TratamientoDatosRespuestaDto
            {
                Id = t.TrId,
                FinalidadId = t.FiId,
                UsuarioId = t.UsId,
                Codigo = t.TrCodigo,
                Nombre = t.TrNombre,
                Modulo = t.TrModulo,
                Descripcion = t.TrDescri,
                CategoriasTitulares = t.TrCategTitu,
                BaseLegitimacion = t.TrBaseLegit,
                PlazoConservacion = t.TrPlazoConse,
                MedidasSeguridad = t.TrMediSegur,
                FechaRevision = t.TrFechaRevi,
                Estatus = t.TrEstatus
            };
        }
    }
}
