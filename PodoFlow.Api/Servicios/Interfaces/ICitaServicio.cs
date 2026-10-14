
using PodoFlow.Api.DTOs.Cita;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface ICitaServicio
{
    Task<List<CitaRespuestaDto>> ObtenerTodosAsync(
        long? pacienteId,
        long? usuarioId,
        string? estado,
        int pagina,
        int tamano);

    Task<CitaRespuestaDto?> ObtenerPorIdAsync(long id);

    Task<CitaRespuestaDto> CrearAsync(CitaCrearDto dto);

    Task<bool> ActualizarAsync(
        long id, CitaActualizarDto dto);

    Task<bool> CambiarEstadoAsync(
        long id, CitaCambiarEstadoDto dto);
}
