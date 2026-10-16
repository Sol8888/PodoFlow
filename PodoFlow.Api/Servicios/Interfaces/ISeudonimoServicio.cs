using PodoFlow.Api.DTOs.Seudonimo;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface ISeudonimoServicio
    {
        Task<List<SeudonimoRespuestaDto>> ObtenerTodosAsync(bool incluirInactivos, int pagina, int tamano);

        Task<SeudonimoRespuestaDto?> ObtenerPorIdAsync(Guid id);

        Task<SeudonimoRespuestaDto?> ObtenerPorPacienteAsync(long pacienteId);

        Task<SeudonimoRespuestaDto> CrearAsync(SeudonimoCrearDto dto);

        Task<SeudonimoGeneracionRespuestaDto> GenerarAsync();

        Task<bool> EliminarAsync(Guid id);

        Task<bool> ReactivarAsync(Guid id);
    }
}
