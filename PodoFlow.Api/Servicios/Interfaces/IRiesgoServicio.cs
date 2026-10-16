using PodoFlow.Api.DTOs.Riesgo;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface IRiesgoServicio
    {
        Task<List<RiesgoRespuestaDto>> ObtenerTodosAsync(int? tratamientoId, string? estado, int pagina, int tamano);

        Task<RiesgoRespuestaDto?> ObtenerPorIdAsync(long id);

        Task<RiesgoRespuestaDto> CrearAsync(RiesgoCrearDto dto);

        Task<bool> ActualizarAsync(long id, RiesgoActualizarDto dto);

        Task<RiesgoRespuestaDto?> CambiarEstadoAsync(long id, RiesgoCambiarEstadoDto dto);
    }
}
