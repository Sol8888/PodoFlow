
using PodoFlow.Api.DTOs.FormaPago;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IFormaPagoServicio
{
    Task<List<FormaPagoRespuestaDto>> ObtenerTodosAsync(
        bool incluirInactivos,
        int pagina,
        int tamano);

    Task<FormaPagoRespuestaDto?> ObtenerPorIdAsync(int id);

    Task<FormaPagoRespuestaDto> CrearAsync(
        FormaPagoCrearDto dto);

    Task<bool> ActualizarAsync(
        int id,
        FormaPagoActualizarDto dto);

    Task<bool> EliminarAsync(int id);

    Task<bool> ReactivarAsync(int id);
}
