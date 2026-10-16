using PodoFlow.Api.DTOs.Encargado;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface IEncargadoServicio
    {
        Task<List<EncargadoRespuestaDto>> ObtenerTodosAsync(bool incluirInactivos, int pagina, int tamano);

        Task<EncargadoRespuestaDto?> ObtenerPorIdAsync(int id);

        Task<EncargadoRespuestaDto> CrearAsync(EncargadoCrearDto dto);

        Task<bool> ActualizarAsync(int id, EncargadoActualizarDto dto);

        Task<bool> EliminarAsync(int id);

        Task<bool> ReactivarAsync(int id);
    }
}
