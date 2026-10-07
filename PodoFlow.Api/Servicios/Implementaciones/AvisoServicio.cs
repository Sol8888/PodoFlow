using PodoFlow.Api.DTOs.Aviso;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    public class AvisoServicio : IAvisoServicio
    {
        private readonly IAvisoRepositorio _repositorio;

        public AvisoServicio(IAvisoRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<List<AvisoRespuestaDto>> ObtenerTodosAsync()
        {
            var avisos = await _repositorio.ObtenerTodosAsync();

            return avisos.Select(MapearRespuesta).ToList();
        }

        public async Task<AvisoRespuestaDto?> ObtenerPorIdAsync(int id)
        {
            var aviso = await _repositorio.ObtenerPorIdAsync(id);

            if (aviso == null)
                return null;

            return MapearRespuesta(aviso);
        }

        public async Task<AvisoRespuestaDto> CrearAsync(
            AvisoCrearDto dto)
        {
            ValidarFechas(dto.FechaDesde, dto.FechaHasta);

            var version = dto.Version.Trim();

            var existente = await _repositorio.ObtenerPorVersionAsync(version);

            if (existente != null)
            {
                if (existente.AvEstatus)
                    throw new InvalidOperationException(
                        $"Ya existe un aviso activo con la versión '{version}'.");

                existente.AvFechaDesd = dto.FechaDesde;
                existente.AvFechaHast = dto.FechaHasta;
                existente.AvRutaArch = dto.RutaArchivo?.Trim();
                existente.AvHuella = dto.Huella?.Trim();
                existente.AvEstatus = true;

                await _repositorio.ActualizarAsync(existente);

                return MapearRespuesta(existente);
            }

            var aviso = new Aviso
            {
                AvVersi = version,
                AvFechaDesd = dto.FechaDesde,
                AvFechaHast = dto.FechaHasta,
                AvRutaArch = dto.RutaArchivo?.Trim(),
                AvHuella = dto.Huella?.Trim(),
                AvEstatus = true
            };

            var creado = await _repositorio.CrearAsync(aviso);

            return MapearRespuesta(creado);
        }

        public async Task<bool> ActualizarAsync(
            int id,
            AvisoActualizarDto dto)
        {
            ValidarFechas(dto.FechaDesde, dto.FechaHasta);

            var aviso = await _repositorio.ObtenerPorIdAsync(id);

            if (aviso == null)
                return false;

            aviso.AvFechaDesd = dto.FechaDesde;
            aviso.AvFechaHast = dto.FechaHasta;
            aviso.AvRutaArch = dto.RutaArchivo?.Trim();
            aviso.AvHuella = dto.Huella?.Trim();
            aviso.AvEstatus = dto.Estatus;

            await _repositorio.ActualizarAsync(aviso);

            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var aviso = await _repositorio.ObtenerPorIdAsync(id);

            if (aviso == null)
                return false;

            await _repositorio.EliminarAsync(aviso);

            return true;
        }

        private static void ValidarFechas(DateOnly desde, DateOnly? hasta)
        {
            if (hasta.HasValue && hasta.Value < desde)
                throw new ArgumentException(
                    "La fecha hasta no puede ser anterior a la fecha desde.");
        }

        private static AvisoRespuestaDto MapearRespuesta(
            Aviso aviso)
        {
            return new AvisoRespuestaDto
            {
                Id = aviso.AvId,
                Version = aviso.AvVersi,
                FechaDesde = aviso.AvFechaDesd,
                FechaHasta = aviso.AvFechaHast,
                RutaArchivo = aviso.AvRutaArch,
                Huella = aviso.AvHuella,
                Estatus = aviso.AvEstatus
            };
        }
    }
}