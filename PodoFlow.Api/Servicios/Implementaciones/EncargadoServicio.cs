using PodoFlow.Api.DTOs.Encargado;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    public class EncargadoServicio : IEncargadoServicio
    {
        private readonly IEncargadoRepositorio _repositorio;

        public EncargadoServicio(IEncargadoRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<List<EncargadoRespuestaDto>> ObtenerTodosAsync(
            bool incluirInactivos, int pagina, int tamano)
        {
            var encargados = await _repositorio.ObtenerTodosAsync(incluirInactivos, pagina, tamano);

            return encargados.Select(MapearRespuesta).ToList();
        }

        public async Task<EncargadoRespuestaDto?> ObtenerPorIdAsync(int id)
        {
            var encargado = await _repositorio.ObtenerPorIdAsync(id);

            return encargado == null ? null : MapearRespuesta(encargado);
        }

        public async Task<EncargadoRespuestaDto> CrearAsync(
            EncargadoCrearDto dto)
        {
            var nombre = dto.Nombre.Trim();
            var servicio = dto.Servicio.Trim();

            await ValidarDuplicadoAsync(nombre, servicio, null);

            var encargado = new Encargado
            {
                EnNombre = nombre,
                EnServi = servicio,
                EnRolTrata = Limpiar(dto.RolTratamiento),
                EnDescri = Limpiar(dto.Descripcion),
                EnRiesgo = Limpiar(dto.Riesgo),
                EnMediContr = Limpiar(dto.MedidasContractuales),
                EnMediTecni = Limpiar(dto.MedidasTecnicas),
                EnEstatus = true
            };

            var creado = await _repositorio.CrearAsync(encargado);

            return MapearRespuesta(creado);
        }

        public async Task<bool> ActualizarAsync(
            int id,
            EncargadoActualizarDto dto)
        {
            var encargado = await _repositorio.ObtenerPorIdAsync(id);

            if (encargado == null)
                return false;

            var nombre = dto.Nombre.Trim();
            var servicio = dto.Servicio.Trim();

            await ValidarDuplicadoAsync(nombre, servicio, id);

            encargado.EnNombre = nombre;
            encargado.EnServi = servicio;
            encargado.EnRolTrata = Limpiar(dto.RolTratamiento);
            encargado.EnDescri = Limpiar(dto.Descripcion);
            encargado.EnRiesgo = Limpiar(dto.Riesgo);
            encargado.EnMediContr = Limpiar(dto.MedidasContractuales);
            encargado.EnMediTecni = Limpiar(dto.MedidasTecnicas);

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var encargado = await _repositorio.ObtenerPorIdAsync(id);

            if (encargado == null)
                return false;

            if (!encargado.EnEstatus)
                return true;

            encargado.EnEstatus = false;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        public async Task<bool> ReactivarAsync(int id)
        {
            var encargado = await _repositorio.ObtenerPorIdAsync(id);

            if (encargado == null)
                return false;

            if (encargado.EnEstatus)
                return true;

            var duplicado = await _repositorio.ObtenerDuplicadoAsync(encargado.EnNombre, encargado.EnServi, id);

            if (duplicado != null && duplicado.EnEstatus)
                throw new InvalidOperationException(
                    $"Ya hay un encargado activo igual (id {duplicado.EnId}). Dé de baja ese antes de reactivar este.");

            encargado.EnEstatus = true;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        private async Task ValidarDuplicadoAsync(string nombre, string servicio, int? excluirId)
        {
            var duplicado = await _repositorio.ObtenerDuplicadoAsync(nombre, servicio, excluirId);

            if (duplicado == null)
                return;

            if (duplicado.EnEstatus)
                throw new InvalidOperationException(
                    $"Ya existe el encargado '{duplicado.EnNombre}' para '{duplicado.EnServi}' (id {duplicado.EnId}).");

            throw new InvalidOperationException(
                $"Ya existe el encargado '{duplicado.EnNombre}' para '{duplicado.EnServi}' dado de baja (id {duplicado.EnId}). " +
                $"Reactívelo con PATCH api/Encargado/{duplicado.EnId}/reactivar.");
        }

        private static string? Limpiar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private static EncargadoRespuestaDto MapearRespuesta(
            Encargado encargado)
        {
            return new EncargadoRespuestaDto
            {
                Id = encargado.EnId,
                Nombre = encargado.EnNombre,
                Servicio = encargado.EnServi,
                RolTratamiento = encargado.EnRolTrata,
                Descripcion = encargado.EnDescri,
                Riesgo = encargado.EnRiesgo,
                MedidasContractuales = encargado.EnMediContr,
                MedidasTecnicas = encargado.EnMediTecni,
                Estatus = encargado.EnEstatus
            };
        }
    }
}