
using PodoFlow.Api.DTOs.HorarioAtencion;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IHorarioAtencionServicio
{
    Task<List<HorarioAtencionRespuestaDto>> ObtenerTodosAsync(
        bool incluirInactivos,
        int pagina,
        int tamano);

    Task<HorarioAtencionRespuestaDto?> ObtenerPorIdAsync(
        int id);

    Task<HorarioAtencionRespuestaDto> CrearAsync(
        HorarioAtencionCrearDto dto);

    Task<bool> ActualizarAsync(
        int id,
        HorarioAtencionActualizarDto dto);

    Task<bool> EliminarAsync(int id);

    Task<bool> ReactivarAsync(int id);
}
