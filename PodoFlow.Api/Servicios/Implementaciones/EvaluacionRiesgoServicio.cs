using PodoFlow.Api.DTOs.EvaluacionRiesgo;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    // Caso C. Como no hay baja, solo se agregan riesgos a evaluaciones en BORRADOR:
    // una evaluación aprobada queda tal como se aprobó, y los cambios van en una versión nueva.
    // Convención: null → 404 · ArgumentException → 400 · InvalidOperationException → 409.
    public class EvaluacionRiesgoServicio : IEvaluacionRiesgoServicio
    {
        private const string Borrador = "BORRADOR";
        private const string RiesgoCerrado = "CERRADO";

        private readonly IEvaluacionRiesgoRepositorio _repositorio;
        private readonly IEvaluacionImpactoRepositorio _evaluacionRepositorio;
        private readonly IRiesgoRepositorio _riesgoRepositorio;

        public EvaluacionRiesgoServicio(
            IEvaluacionRiesgoRepositorio repositorio,
            IEvaluacionImpactoRepositorio evaluacionRepositorio,
            IRiesgoRepositorio riesgoRepositorio)
        {
            _repositorio = repositorio;
            _evaluacionRepositorio = evaluacionRepositorio;
            _riesgoRepositorio = riesgoRepositorio;
        }

        public async Task<List<EvaluacionRiesgoRespuestaDto>> ObtenerTodosAsync(
            long? evaluacionId, long? riesgoId, int pagina, int tamano)
        {
            var asignaciones = await _repositorio.ObtenerTodosAsync(evaluacionId, riesgoId, pagina, tamano);

            return asignaciones.Select(MapearRespuesta).ToList();
        }

        public async Task<EvaluacionRiesgoRespuestaDto?> ObtenerAsync(long evaluacionId, long riesgoId)
        {
            var asignacion = await _repositorio.ObtenerAsync(evaluacionId, riesgoId);

            return asignacion == null ? null : MapearRespuesta(asignacion);
        }

        public async Task<EvaluacionRiesgoRespuestaDto> CrearAsync(
            EvaluacionRiesgoCrearDto dto)
        {
            await ValidarEvaluacionEnBorradorAsync(dto.EvaluacionId);

            var riesgo = await _riesgoRepositorio.ObtenerPorIdAsync(dto.RiesgoId);

            if (riesgo == null)
                throw new ArgumentException($"No existe el riesgo {dto.RiesgoId}.");

            // Un riesgo cerrado ya no aplica: no tiene sentido evaluarlo en una EIPD nueva.
            if (riesgo.RiEstad == RiesgoCerrado)
                throw new InvalidOperationException($"El riesgo {dto.RiesgoId} está CERRADO.");

            // PK (ev_id, ri_id): la misma pareja no se repite.
            if (await _repositorio.ObtenerAsync(dto.EvaluacionId, dto.RiesgoId) != null)
                throw new InvalidOperationException("El riesgo ya está incluido en esta evaluación.");

            var asignacion = new EvaluacionRiesgo
            {
                EvId = dto.EvaluacionId,
                RiId = dto.RiesgoId,
                ErFechaAsig = DateTime.UtcNow,
                ErObser = string.IsNullOrWhiteSpace(dto.Observacion) ? null : dto.Observacion.Trim()
            };

            var creada = await _repositorio.CrearAsync(asignacion);

            return MapearRespuesta(creada);
        }

        public async Task<List<EvaluacionRiesgoRespuestaDto>> CopiarAsync(
            EvaluacionRiesgoCopiarDto dto)
        {
            if (dto.EvaluacionOrigenId == dto.EvaluacionDestinoId)
                throw new ArgumentException("La evaluación origen y la destino deben ser distintas.");

            var origen = await _evaluacionRepositorio.ObtenerPorIdAsync(dto.EvaluacionOrigenId);

            if (origen == null)
                throw new ArgumentException($"No existe la evaluación {dto.EvaluacionOrigenId}.");

            await ValidarEvaluacionEnBorradorAsync(dto.EvaluacionDestinoId);

            var excluidos = dto.RiesgosExcluidos.ToHashSet();
            var deOrigen = await _repositorio.ObtenerPorEvaluacionAsync(dto.EvaluacionOrigenId);
            var yaEnDestino = (await _repositorio.ObtenerPorEvaluacionAsync(dto.EvaluacionDestinoId))
                .Select(x => x.RiId)
                .ToHashSet();

            var ahora = DateTime.UtcNow;
            var nuevas = new List<EvaluacionRiesgo>();

            foreach (var item in deOrigen)
            {
                // Se saltan los excluidos y los que ya estén en el destino.
                if (excluidos.Contains(item.RiId) || yaEnDestino.Contains(item.RiId))
                    continue;

                // Los riesgos que se cerraron desde la versión anterior tampoco pasan.
                var riesgo = await _riesgoRepositorio.ObtenerPorIdAsync(item.RiId);

                if (riesgo == null || riesgo.RiEstad == RiesgoCerrado)
                    continue;

                nuevas.Add(new EvaluacionRiesgo
                {
                    EvId = dto.EvaluacionDestinoId,
                    RiId = item.RiId,
                    ErFechaAsig = ahora,
                    ErObser = item.ErObser
                });
            }

            if (nuevas.Count > 0)
                await _repositorio.CrearVariosAsync(nuevas);

            return nuevas.Select(MapearRespuesta).ToList();
        }

        private async Task ValidarEvaluacionEnBorradorAsync(long evaluacionId)
        {
            var evaluacion = await _evaluacionRepositorio.ObtenerPorIdAsync(evaluacionId);

            if (evaluacion == null)
                throw new ArgumentException($"No existe la evaluación {evaluacionId}.");

            if (evaluacion.EvEstad != Borrador)
                throw new InvalidOperationException(
                    $"La evaluación {evaluacionId} está {evaluacion.EvEstad}. Solo se le agregan riesgos en BORRADOR; " +
                    "para cambiarla cree una versión nueva.");
        }

        private static EvaluacionRiesgoRespuestaDto MapearRespuesta(
            EvaluacionRiesgo asignacion)
        {
            return new EvaluacionRiesgoRespuestaDto
            {
                EvaluacionId = asignacion.EvId,
                RiesgoId = asignacion.RiId,
                FechaAsignacion = asignacion.ErFechaAsig,
                Observacion = asignacion.ErObser
            };
        }
    }
}
