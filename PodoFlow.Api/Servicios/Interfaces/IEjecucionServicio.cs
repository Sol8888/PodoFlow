using PodoFlow.Api.DTOs.Ejecucion;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface IEjecucionServicio
    {
        Task<List<EjecucionRespuestaDto>> ObtenerTodosAsync(string? estado, int pagina, int tamano);

        Task<EjecucionRespuestaDto?> ObtenerPorIdAsync(long id);

        Task<List<DatoAnaliticoRespuestaDto>?> ObtenerDatosAsync(long id, int pagina, int tamano);

        Task<List<RecomendacionRespuestaDto>?> ObtenerRecomendacionesAsync(long id);

        Task<EjecucionRespuestaDto> CrearAsync(EjecucionCrearDto dto);

        Task<bool> ActualizarAsync(long id, EjecucionActualizarDto dto);

        Task<EjecucionRespuestaDto?> CambiarEstadoAsync(long id, EjecucionCambiarEstadoDto dto);

        Task<int?> CargarDatosAsync(long id, DatoAnaliticoLoteDto dto);

        Task<RecomendacionRespuestaDto?> CrearRecomendacionAsync(long id, RecomendacionCrearDto dto);
    }
}
