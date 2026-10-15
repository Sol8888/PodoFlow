
using PodoFlow.Api.DTOs.Movimiento;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IMovimientoServicio
{
    Task<List<MovimientoRespuestaDto>> ObtenerTodosAsync(
        int? insumoId,
        long? atencionId,
        string? tipo,
        int pagina,
        int tamano);

    Task<MovimientoRespuestaDto?> ObtenerPorIdAsync(long id);

    Task<MovimientoRespuestaDto> CrearAsync(
        MovimientoCrearDto dto);
}
