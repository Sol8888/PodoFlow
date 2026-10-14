
using PodoFlow.Api.DTOs.CambioCita;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface ICambioCitaServicio
{
    Task<CambioCitaRespuestaDto> CrearAsync(
        CambioCitaCrearDto dto);

    Task<CambioCitaRespuestaDto?> ObtenerPorIdAsync(long id);

    Task<List<CambioCitaRespuestaDto>> ObtenerTodosAsync(
        long? citaId,
        int pagina,
        int tamano);
}
