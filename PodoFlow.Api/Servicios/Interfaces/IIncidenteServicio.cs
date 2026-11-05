using PodoFlow.Api.DTOs.Incidente;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface IIncidenteServicio
    {
        Task<List<IncidenteRespuestaDto>> ObtenerTodosAsync(string? estado, int pagina, int tamano);

        Task<IncidenteRespuestaDto?> ObtenerPorIdAsync(long id);

        Task<List<AccionIncidenteRespuestaDto>?> ObtenerAccionesAsync(long id);

        Task<IncidenteRespuestaDto> CrearAsync(IncidenteCrearDto dto);

        Task<bool> ActualizarAsync(long id, IncidenteActualizarDto dto);

        Task<IncidenteRespuestaDto?> CambiarEstadoAsync(long id, IncidenteCambiarEstadoDto dto);

        Task<AccionIncidenteRespuestaDto?> RegistrarAccionAsync(long id, AccionIncidenteCrearDto dto);
    }
}
