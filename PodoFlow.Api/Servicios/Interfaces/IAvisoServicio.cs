using PodoFlow.Api.DTOs.Aviso;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface IAvisoServicio
    {
        Task<List<AvisoRespuestaDto>> ObtenerTodosAsync();

        Task<AvisoRespuestaDto?> ObtenerPorIdAsync(int id);

        Task<AvisoRespuestaDto> CrearAsync(AvisoCrearDto dto);

        Task<bool> ActualizarAsync(int id, AvisoActualizarDto dto);

        Task<bool> EliminarAsync(int id);
    }
}
