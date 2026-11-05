using PodoFlow.Api.DTOs.TratamientoEncargado;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface ITratamientoEncargadoServicio
    {
        Task<List<TratamientoEncargadoRespuestaDto>> ObtenerTodosAsync(
            int? tratamientoId, int? encargadoId, bool incluirInactivos, int pagina, int tamano);

        Task<TratamientoEncargadoRespuestaDto?> ObtenerAsync(int tratamientoId, int encargadoId);

        Task<TratamientoEncargadoRespuestaDto> CrearAsync(TratamientoEncargadoCrearDto dto);

        Task<bool> EliminarAsync(int tratamientoId, int encargadoId);

        Task<bool> ReactivarAsync(int tratamientoId, int encargadoId);
    }
}
