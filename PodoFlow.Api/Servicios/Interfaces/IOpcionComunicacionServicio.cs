using PodoFlow.Api.DTOs.OpcionComunicacion;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface IOpcionComunicacionServicio
    {
        Task<List<OpcionComunicacionRespuestaDto>> ObtenerTodosAsync(
            long? pacienteId, int? finalidadId, string? canal, bool incluirInactivos, int pagina, int tamano);

        Task<OpcionComunicacionRespuestaDto?> ObtenerPorIdAsync(long id);

        Task<OpcionComunicacionRespuestaDto> CrearAsync(OpcionComunicacionCrearDto dto);

        Task<bool> ActualizarAsync(long id, OpcionComunicacionActualizarDto dto);

        Task<bool> EliminarAsync(long id);

        Task<bool> ReactivarAsync(long id);
    }
}
