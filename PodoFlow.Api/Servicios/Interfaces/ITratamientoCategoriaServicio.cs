using PodoFlow.Api.DTOs.TratamientoCategoria;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface ITratamientoCategoriaServicio
    {
        Task<List<TratamientoCategoriaRespuestaDto>> ObtenerTodosAsync(
            int? tratamientoId, int? categoriaId, bool incluirInactivos, int pagina, int tamano);

        Task<TratamientoCategoriaRespuestaDto?> ObtenerAsync(int tratamientoId, int categoriaId);

        Task<TratamientoCategoriaRespuestaDto> CrearAsync(TratamientoCategoriaCrearDto dto);

        Task<bool> EliminarAsync(int tratamientoId, int categoriaId);

        Task<bool> ReactivarAsync(int tratamientoId, int categoriaId);
    }
}
