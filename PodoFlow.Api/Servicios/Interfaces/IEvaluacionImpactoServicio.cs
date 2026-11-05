using PodoFlow.Api.DTOs.EvaluacionImpacto;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface IEvaluacionImpactoServicio
    {
        Task<List<EvaluacionImpactoRespuestaDto>> ObtenerTodosAsync(string? estado, int pagina, int tamano);

        Task<EvaluacionImpactoRespuestaDto?> ObtenerPorIdAsync(long id);

        Task<EvaluacionImpactoRespuestaDto> CrearAsync(EvaluacionImpactoCrearDto dto);

        Task<bool> ActualizarAsync(long id, EvaluacionImpactoActualizarDto dto);

        Task<EvaluacionImpactoRespuestaDto?> CambiarEstadoAsync(long id, EvaluacionImpactoCambiarEstadoDto dto);
    }
}
