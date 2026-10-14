using PodoFlow.Api.DTOs.Exportacion;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface IExportacionServicio
    {
        Task<List<ExportacionRespuestaDto>> ObtenerTodosAsync(int pagina, int tamano);

        Task<List<ExportacionRespuestaDto>?> ObtenerPorSolicitudAsync(long solicitudId);

        Task<ExportacionRespuestaDto?> ObtenerPorIdAsync(long id);

        Task<ExportacionRespuestaDto> CrearAsync(ExportacionCrearDto dto);

        Task<ExportacionRespuestaDto?> CambiarEstadoAsync(long id, ExportacionCambiarEstadoDto dto);
    }
}