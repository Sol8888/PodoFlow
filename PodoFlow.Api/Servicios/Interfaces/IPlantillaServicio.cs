using PodoFlow.Api.DTOs.Plantilla;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface IPlantillaServicio
    {
        Task<List<PlantillaRespuestaDto>> ObtenerTodosAsync(
            string? canal, string? tipo, bool incluirInactivos, int pagina, int tamano);

        Task<PlantillaRespuestaDto?> ObtenerPorIdAsync(int id);

        Task<PlantillaRespuestaDto> CrearAsync(PlantillaCrearDto dto);

        Task<bool> ActualizarAsync(int id, PlantillaActualizarDto dto);

        Task<bool> EliminarAsync(int id);

        Task<bool> ReactivarAsync(int id);
    }
}
