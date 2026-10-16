using PodoFlow.Api.DTOs.EvaluacionImpacto;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    // Caso B: la "baja" es CERRAR.
    // Una EIPD aprobada es un documento firme: ya no se edita. Si hay que cambiarla,
    // se crea una versión nueva (ev_versi), que es lo que indica el documento para EV_RI.
    // Convención: null/false → 404 · ArgumentException → 400 · InvalidOperationException → 409.
    public class EvaluacionImpactoServicio : IEvaluacionImpactoServicio
    {
        private const string Borrador = "BORRADOR";
        private const string Aprobada = "APROBADA";
        private const string Revisada = "REVISADA";
        private const string Cerrada = "CERRADA";

        // Estado actual → estados a los que puede pasar. CERRADA es final.
        private static readonly Dictionary<string, string[]> TransicionesPermitidas = new()
        {
            [Borrador] = [Aprobada, Cerrada],   // cerrar un borrador = descartarlo
            [Aprobada] = [Revisada, Cerrada],
            [Revisada] = [Cerrada],
            [Cerrada] = []
        };

        private readonly IEvaluacionImpactoRepositorio _repositorio;

        public EvaluacionImpactoServicio(IEvaluacionImpactoRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<List<EvaluacionImpactoRespuestaDto>> ObtenerTodosAsync(
            string? estado, int pagina, int tamano)
        {
            string? filtroEstado = null;

            if (!string.IsNullOrWhiteSpace(estado))
            {
                filtroEstado = Normalizar(estado);

                if (!TransicionesPermitidas.ContainsKey(filtroEstado))
                    throw new ArgumentException("El estado debe ser BORRADOR, APROBADA, REVISADA o CERRADA.");
            }

            var evaluaciones = await _repositorio.ObtenerTodosAsync(filtroEstado, pagina, tamano);

            return evaluaciones.Select(MapearRespuesta).ToList();
        }

        public async Task<EvaluacionImpactoRespuestaDto?> ObtenerPorIdAsync(long id)
        {
            var evaluacion = await _repositorio.ObtenerPorIdAsync(id);

            return evaluacion == null ? null : MapearRespuesta(evaluacion);
        }

        public async Task<EvaluacionImpactoRespuestaDto> CrearAsync(
            EvaluacionImpactoCrearDto dto)
        {
            if (!await _repositorio.ExisteUsuarioActivoAsync(dto.UsuarioId))
                throw new ArgumentException($"No existe un usuario activo con id {dto.UsuarioId}.");

            var evaluacion = new EvaluacionImpacto
            {
                UsId = dto.UsuarioId,
                EvVersi = dto.Version.Trim(),
                EvFecha = dto.Fecha,
                EvAlcan = dto.Alcance.Trim(),
                EvDescri = Limpiar(dto.Descripcion),
                EvConclu = Limpiar(dto.Conclusion),
                EvNivelResid = dto.NivelResidual,
                EvEstad = Borrador
            };

            var creada = await _repositorio.CrearAsync(evaluacion);

            return MapearRespuesta(creada);
        }

        public async Task<bool> ActualizarAsync(
            long id,
            EvaluacionImpactoActualizarDto dto)
        {
            var evaluacion = await _repositorio.ObtenerPorIdAsync(id);

            if (evaluacion == null)
                return false;

            if (evaluacion.EvEstad != Borrador)
                throw new InvalidOperationException(
                    $"La evaluación está {evaluacion.EvEstad}. Solo se edita en BORRADOR; para cambiarla cree una versión nueva.");

            evaluacion.EvVersi = dto.Version.Trim();
            evaluacion.EvFecha = dto.Fecha;
            evaluacion.EvAlcan = dto.Alcance.Trim();
            evaluacion.EvDescri = Limpiar(dto.Descripcion);
            evaluacion.EvConclu = Limpiar(dto.Conclusion);
            evaluacion.EvNivelResid = dto.NivelResidual;
            evaluacion.EvFechaModi = DateTime.UtcNow;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        public async Task<EvaluacionImpactoRespuestaDto?> CambiarEstadoAsync(
            long id,
            EvaluacionImpactoCambiarEstadoDto dto)
        {
            var nuevoEstado = Normalizar(dto.Estado);

            if (!TransicionesPermitidas.ContainsKey(nuevoEstado))
                throw new ArgumentException("El estado debe ser BORRADOR, APROBADA, REVISADA o CERRADA.");

            var evaluacion = await _repositorio.ObtenerPorIdAsync(id);

            if (evaluacion == null)
                return null;

            if (!TransicionesPermitidas[evaluacion.EvEstad].Contains(nuevoEstado))
                throw new InvalidOperationException(
                    $"No se puede pasar de {evaluacion.EvEstad} a {nuevoEstado}.");

            if (nuevoEstado == Aprobada)
            {
                if (string.IsNullOrWhiteSpace(evaluacion.EvConclu))
                    throw new ArgumentException("Para aprobarla hay que registrar la conclusión.");

                if (!evaluacion.EvNivelResid.HasValue)
                    throw new ArgumentException("Para aprobarla hay que registrar el nivel de riesgo residual.");
            }

            evaluacion.EvEstad = nuevoEstado;
            evaluacion.EvFechaModi = DateTime.UtcNow;

            await _repositorio.GuardarCambiosAsync();

            return MapearRespuesta(evaluacion);
        }

        private static string Normalizar(string valor)
        {
            return valor.Trim().ToUpperInvariant();
        }
        private static string? Limpiar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private static EvaluacionImpactoRespuestaDto MapearRespuesta(
            EvaluacionImpacto evaluacion)
        {
            return new EvaluacionImpactoRespuestaDto
            {
                Id = evaluacion.EvId,
                UsuarioId = evaluacion.UsId,
                Version = evaluacion.EvVersi,
                Fecha = evaluacion.EvFecha,
                Alcance = evaluacion.EvAlcan,
                Descripcion = evaluacion.EvDescri,
                Conclusion = evaluacion.EvConclu,
                NivelResidual = evaluacion.EvNivelResid,
                Estado = evaluacion.EvEstad,
                FechaModificacion = evaluacion.EvFechaModi
            };
        }
    }
}
