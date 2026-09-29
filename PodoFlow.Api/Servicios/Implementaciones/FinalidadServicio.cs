using PodoFlow.Api.DTOs.Finalidad;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    public class FinalidadServicio : IFinalidadServicio
    {
        private readonly IFinalidadRepositorio _repositorio;

        public FinalidadServicio(IFinalidadRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<List<FinalidadRespuestaDto>> ObtenerTodosAsync()
        {
            var finalidades = await _repositorio.ObtenerTodosAsync();

            return finalidades.Select(MapearRespuesta).ToList();
        }

        public async Task<FinalidadRespuestaDto?> ObtenerPorIdAsync(int id)
        {
            var finalidad = await _repositorio.ObtenerPorIdAsync(id);

            if (finalidad == null)
                return null;

            return MapearRespuesta(finalidad);
        }

        public async Task<FinalidadRespuestaDto> CrearAsync(
            FinalidadCrearDto dto)
        {
            var codigo = dto.Codigo.Trim().ToUpperInvariant();

            var existente = await _repositorio.ObtenerPorCodigoAsync(codigo);

            if (existente != null)
            {
                if (existente.FiEstatus)
                    throw new InvalidOperationException(
                        $"Ya existe una finalidad activa con el código '{codigo}'.");

                existente.FiNombre = dto.Nombre.Trim();
                existente.FiDescri = dto.Descripcion?.Trim();
                existente.FiRequiCons = dto.RequiereConsentimiento;
                existente.FiEstatus = true;

                await _repositorio.ActualizarAsync(existente);

                return MapearRespuesta(existente);
            }

            var finalidad = new Finalidad
            {
                FiCodigo = codigo,
                FiNombre = dto.Nombre.Trim(),
                FiDescri = dto.Descripcion?.Trim(),
                FiRequiCons = dto.RequiereConsentimiento,
                FiEstatus = true
            };

            var creada = await _repositorio.CrearAsync(finalidad);

            return MapearRespuesta(creada);
        }

        public async Task<bool> ActualizarAsync(
            int id,
            FinalidadActualizarDto dto)
        {
            var finalidad = await _repositorio.ObtenerPorIdAsync(id);

            if (finalidad == null)
                return false;

            finalidad.FiNombre = dto.Nombre.Trim();
            finalidad.FiDescri = dto.Descripcion?.Trim();
            finalidad.FiRequiCons = dto.RequiereConsentimiento;
            finalidad.FiEstatus = dto.Estatus;

            await _repositorio.ActualizarAsync(finalidad);

            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var finalidad = await _repositorio.ObtenerPorIdAsync(id);

            if (finalidad == null)
                return false;

            await _repositorio.EliminarAsync(finalidad);

            return true;
        }
        private static FinalidadRespuestaDto MapearRespuesta(Finalidad finalidad)
        {
            return new FinalidadRespuestaDto
            {
                Id = finalidad.FiId,
                Codigo = finalidad.FiCodigo,
                Nombre = finalidad.FiNombre,
                Descripcion = finalidad.FiDescri,
                RequiereConsentimiento = finalidad.FiRequiCons,
                Estatus = finalidad.FiEstatus
            };
        }
    }
}
    
