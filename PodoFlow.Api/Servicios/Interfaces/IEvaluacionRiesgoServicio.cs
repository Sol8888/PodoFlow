using PodoFlow.Api.DTOs.EvaluacionRiesgo;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface IEvaluacionRiesgoServicio
    {
        Task<List<EvaluacionRiesgoRespuestaDto>> ObtenerTodosAsync(long? evaluacionId, long? riesgoId, int pagina, int tamano);

        Task<EvaluacionRiesgoRespuestaDto?> ObtenerAsync(long evaluacionId, long riesgoId);

        Task<EvaluacionRiesgoRespuestaDto> CrearAsync(EvaluacionRiesgoCrearDto dto);

        Task<List<EvaluacionRiesgoRespuestaDto>> CopiarAsync(EvaluacionRiesgoCopiarDto dto);
    }
}
