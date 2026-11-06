using PodoFlow.Api.DTOs.Riesgo;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    
    public class RiesgoServicio : IRiesgoServicio
    {
        private const string Abierto = "ABIERTO";
        private const string Mitigado = "MITIGADO";
        private const string Aceptado = "ACEPTADO";
        private const string Cerrado = "CERRADO";

      
        private static readonly Dictionary<string, string[]> TransicionesPermitidas = new()
        {
            [Abierto] = [Mitigado, Aceptado, Cerrado],
            [Mitigado] = [Abierto, Aceptado, Cerrado],  
            [Aceptado] = [Abierto, Cerrado],            
            [Cerrado] = []
        };

        private readonly IRiesgoRepositorio _repositorio;
        private readonly ITratamientoDatosRepositorio _tratamientoRepositorio;

        public RiesgoServicio(
            IRiesgoRepositorio repositorio,
            ITratamientoDatosRepositorio tratamientoRepositorio)
        {
            _repositorio = repositorio;
            _tratamientoRepositorio = tratamientoRepositorio;
        }

        public async Task<List<RiesgoRespuestaDto>> ObtenerTodosAsync(
            int? tratamientoId, string? estado, int pagina, int tamano)
        {
            string? filtroEstado = null;

            if (!string.IsNullOrWhiteSpace(estado))
            {
                filtroEstado = Normalizar(estado);

                if (!TransicionesPermitidas.ContainsKey(filtroEstado))
                    throw new ArgumentException("El estado debe ser ABIERTO, MITIGADO, ACEPTADO o CERRADO.");
            }

            var riesgos = await _repositorio.ObtenerTodosAsync(tratamientoId, filtroEstado, pagina, tamano);

            return riesgos.Select(MapearRespuesta).ToList();
        }

        public async Task<RiesgoRespuestaDto?> ObtenerPorIdAsync(long id)
        {
            var riesgo = await _repositorio.ObtenerPorIdAsync(id);

            return riesgo == null ? null : MapearRespuesta(riesgo);
        }

        public async Task<RiesgoRespuestaDto> CrearAsync(
            RiesgoCrearDto dto)
        {
            var tratamiento = await _tratamientoRepositorio.ObtenerPorIdAsync(dto.TratamientoId);

            if (tratamiento == null || !tratamiento.TrEstatus)
                throw new ArgumentException($"No existe un tratamiento activo con id {dto.TratamientoId}.");

            var nivel = CalcularNivel(dto.Probabilidad, dto.Impacto);

            ValidarResidual(dto.RiesgoResidual, nivel);

            var riesgo = new Riesgo
            {
                TrId = dto.TratamientoId,
                RiDescri = dto.Descripcion.Trim(),
                RiProba = dto.Probabilidad,
                RiImpac = dto.Impacto,
                RiNivel = nivel,
                RiMediMitig = Limpiar(dto.MedidasMitigacion),
                RiRiesgResid = dto.RiesgoResidual,
                RiEstad = Abierto,
                RiFechaRevi = dto.FechaRevision
            };

            var creado = await _repositorio.CrearAsync(riesgo);

            return MapearRespuesta(creado);
        }

        public async Task<bool> ActualizarAsync(
            long id,
            RiesgoActualizarDto dto)
        {
            var riesgo = await _repositorio.ObtenerPorIdAsync(id);

            if (riesgo == null)
                return false;

            if (riesgo.RiEstad == Cerrado)
                throw new InvalidOperationException("El riesgo está CERRADO y ya no se puede modificar.");

            var nivel = CalcularNivel(dto.Probabilidad, dto.Impacto);

            ValidarResidual(dto.RiesgoResidual, nivel);

            riesgo.RiDescri = dto.Descripcion.Trim();
            riesgo.RiProba = dto.Probabilidad;
            riesgo.RiImpac = dto.Impacto;
            riesgo.RiNivel = nivel;
            riesgo.RiMediMitig = Limpiar(dto.MedidasMitigacion);
            riesgo.RiRiesgResid = dto.RiesgoResidual;
            riesgo.RiFechaRevi = dto.FechaRevision;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        public async Task<RiesgoRespuestaDto?> CambiarEstadoAsync(
            long id,
            RiesgoCambiarEstadoDto dto)
        {
            var nuevoEstado = Normalizar(dto.Estado);

            if (!TransicionesPermitidas.ContainsKey(nuevoEstado))
                throw new ArgumentException("El estado debe ser ABIERTO, MITIGADO, ACEPTADO o CERRADO.");

            var riesgo = await _repositorio.ObtenerPorIdAsync(id);

            if (riesgo == null)
                return null;

            if (!TransicionesPermitidas[riesgo.RiEstad].Contains(nuevoEstado))
                throw new InvalidOperationException(
                    $"No se puede pasar de {riesgo.RiEstad} a {nuevoEstado}.");

            if (!string.IsNullOrWhiteSpace(dto.MedidasMitigacion))
                riesgo.RiMediMitig = dto.MedidasMitigacion.Trim();

            if (dto.RiesgoResidual.HasValue)
            {
                ValidarResidual(dto.RiesgoResidual, riesgo.RiNivel);
                riesgo.RiRiesgResid = dto.RiesgoResidual;
            }

            if (nuevoEstado == Mitigado)
            {
                if (string.IsNullOrWhiteSpace(riesgo.RiMediMitig))
                    throw new ArgumentException("Para marcarlo como MITIGADO hay que indicar las medidas de mitigación.");

                if (!riesgo.RiRiesgResid.HasValue)
                    throw new ArgumentException("Para marcarlo como MITIGADO hay que indicar el riesgo residual.");
            }

            riesgo.RiEstad = nuevoEstado;

            await _repositorio.GuardarCambiosAsync();

            return MapearRespuesta(riesgo);
        }

        private static byte CalcularNivel(byte probabilidad, byte impacto)
        {
            return (byte)(probabilidad * impacto);
        }

        private static void ValidarResidual(byte? residual, byte nivel)
        {
            if (residual.HasValue && residual.Value > nivel)
                throw new ArgumentException(
                    $"El riesgo residual ({residual.Value}) no puede ser mayor que el nivel del riesgo ({nivel}).");
        }

        private static string Normalizar(string valor)
        {
            return valor.Trim().ToUpperInvariant();
        }

        private static string? Limpiar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private static RiesgoRespuestaDto MapearRespuesta(
            Riesgo riesgo)
        {
            return new RiesgoRespuestaDto
            {
                Id = riesgo.RiId,
                TratamientoId = riesgo.TrId,
                Descripcion = riesgo.RiDescri,
                Probabilidad = riesgo.RiProba,
                Impacto = riesgo.RiImpac,
                Nivel = riesgo.RiNivel,
                MedidasMitigacion = riesgo.RiMediMitig,
                RiesgoResidual = riesgo.RiRiesgResid,
                Estado = riesgo.RiEstad,
                FechaRevision = riesgo.RiFechaRevi
            };
        }
    }
}
