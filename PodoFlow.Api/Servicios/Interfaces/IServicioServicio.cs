using PodoFlow.Api.DTOs.Servicio;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IServicioServicio
{
    Task<List<ServicioRespuestaDto>> ObtenerTodosAsync();

    Task<ServicioRespuestaDto?> ObtenerPorIdAsync(int id);

    Task<ServicioRespuestaDto> CrearAsync(
        ServicioCrearDto dto
    );

    Task<bool> ActualizarAsync(
        int id,
        ServicioActualizarDto dto
    );

    Task<bool> EliminarAsync(int id);
}
