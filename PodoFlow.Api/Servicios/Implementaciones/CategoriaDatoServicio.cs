using PodoFlow.Api.DTOs.CategoriaDato;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    public class CategoriaDatoServicio : ICategoriaDatoServicio
    {
        private readonly ICategoriaDatoRepositorio _repositorio;

        public CategoriaDatoServicio(ICategoriaDatoRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<List<CategoriaDatoRespuestaDto>> ObtenerTodosAsync(
            bool incluirInactivos, int pagina, int tamano)
        {
            var categorias = await _repositorio.ObtenerTodosAsync(incluirInactivos, pagina, tamano);

            return categorias.Select(MapearRespuesta).ToList();
        }

        public async Task<CategoriaDatoRespuestaDto?> ObtenerPorIdAsync(int id)
        {
            var categoria = await _repositorio.ObtenerPorIdAsync(id);

            return categoria == null ? null : MapearRespuesta(categoria);
        }

        public async Task<CategoriaDatoRespuestaDto> CrearAsync(
            CategoriaDatoCrearDto dto)
        {
            var nombre = dto.Nombre.Trim();

            var existente = await _repositorio.ObtenerPorNombreAsync(nombre);

            if (existente != null)
            {
                if (existente.CaEstatus)
                    throw new InvalidOperationException(
                        $"Ya existe una categoría activa con el nombre '{existente.CaNombre}'.");

                existente.CaNombre = nombre;
                existente.CaDescri = dto.Descripcion?.Trim();
                existente.CaSensi = dto.EsSensible;
                existente.CaEstatus = true;

                await _repositorio.GuardarCambiosAsync();

                return MapearRespuesta(existente);
            }

            var categoria = new CategoriaDato
            {
                CaNombre = nombre,
                CaDescri = dto.Descripcion?.Trim(),
                CaSensi = dto.EsSensible,
                CaEstatus = true
            };

            var creada = await _repositorio.CrearAsync(categoria);

            return MapearRespuesta(creada);
        }

        public async Task<bool> ActualizarAsync(
            int id,
            CategoriaDatoActualizarDto dto)
        {
            var categoria = await _repositorio.ObtenerPorIdAsync(id);

            if (categoria == null)
                return false;

            var nombre = dto.Nombre.Trim();

            var existente = await _repositorio.ObtenerPorNombreAsync(nombre);

            if (existente != null && existente.CaId != id)
                throw new InvalidOperationException(
                    $"El nombre '{existente.CaNombre}' ya pertenece a otra categoría.");

            categoria.CaNombre = nombre;
            categoria.CaDescri = dto.Descripcion?.Trim();
            categoria.CaSensi = dto.EsSensible;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var categoria = await _repositorio.ObtenerPorIdAsync(id);

            if (categoria == null)
                return false;

            if (!categoria.CaEstatus)
                return true;

            categoria.CaEstatus = false;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        public async Task<bool> ReactivarAsync(int id)
        {
            var categoria = await _repositorio.ObtenerPorIdAsync(id);

            if (categoria == null)
                return false;

            if (categoria.CaEstatus)
                return true;

            categoria.CaEstatus = true;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        private static CategoriaDatoRespuestaDto MapearRespuesta(
            CategoriaDato categoria)
        {
            return new CategoriaDatoRespuestaDto
            {
                Id = categoria.CaId,
                Nombre = categoria.CaNombre,
                Descripcion = categoria.CaDescri,
                EsSensible = categoria.CaSensi,
                Estatus = categoria.CaEstatus
            };
        }
    }
}
