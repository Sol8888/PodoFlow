using PodoFlow.Api.DTOs.TratamientoDatos;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface ITratamientoDatosServicio
    {
        Task<List<TratamientoDatosRespuestaDto>> ObtenerTodosAsync(bool incluirInactivos, int pagina, int tamano);

        Task<TratamientoDatosRespuestaDto?> ObtenerPorIdAsync(int id);

        Task<TratamientoDatosRespuestaDto> CrearAsync(TratamientoDatosCrearDto dto);

        Task<bool> ActualizarAsync(int id, TratamientoDatosActualizarDto dto);

        Task<bool> EliminarAsync(int id);

        Task<bool> ReactivarAsync(int id);
    }
}
