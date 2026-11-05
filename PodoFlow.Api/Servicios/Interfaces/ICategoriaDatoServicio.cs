using PodoFlow.Api.DTOs.CategoriaDato;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface ICategoriaDatoServicio
    {
        Task<List<CategoriaDatoRespuestaDto>> ObtenerTodosAsync(bool incluirInactivos, int pagina, int tamano);

        Task<CategoriaDatoRespuestaDto?> ObtenerPorIdAsync(int id);

        Task<CategoriaDatoRespuestaDto> CrearAsync(CategoriaDatoCrearDto dto);

        Task<bool> ActualizarAsync(int id, CategoriaDatoActualizarDto dto);

        Task<bool> EliminarAsync(int id);

        Task<bool> ReactivarAsync(int id);

    }
}
