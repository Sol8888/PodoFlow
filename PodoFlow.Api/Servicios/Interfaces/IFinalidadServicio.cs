using PodoFlow.Api.DTOs.Finalidad;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface IFinalidadServicio
    {
        Task<List<FinalidadRespuestaDto>> ObtenerTodosAsync();

        Task<FinalidadRespuestaDto?> ObtenerPorIdAsync(int id);

        Task<FinalidadRespuestaDto> CrearAsync(
            FinalidadCrearDto dto
        );

        Task<bool> ActualizarAsync(
            int id,
            FinalidadActualizarDto dto
        );

        Task<bool> EliminarAsync(int id);
    }
}
