using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface IEvaluacionImpactoRepositorio
    {
        Task<List<EvaluacionImpacto>> ObtenerTodosAsync(string? estado, int pagina, int tamano);

        Task<EvaluacionImpacto?> ObtenerPorIdAsync(long id);

        Task<bool> ExisteUsuarioActivoAsync(long usuarioId);

        Task<EvaluacionImpacto> CrearAsync(EvaluacionImpacto evaluacion);

        Task GuardarCambiosAsync();
    }
}
