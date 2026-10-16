using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface IEvaluacionRiesgoRepositorio
    {
        Task<List<EvaluacionRiesgo>> ObtenerTodosAsync(long? evaluacionId, long? riesgoId, int pagina, int tamano);

        Task<List<EvaluacionRiesgo>> ObtenerPorEvaluacionAsync(long evaluacionId);

        Task<EvaluacionRiesgo?> ObtenerAsync(long evaluacionId, long riesgoId);

        Task<EvaluacionRiesgo> CrearAsync(EvaluacionRiesgo asignacion);

        Task CrearVariosAsync(List<EvaluacionRiesgo> asignaciones);
    }
}
