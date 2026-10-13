
using PodoFlow.Api.DTOs.Paciente;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IPacienteServicio
{
    Task<List<PacienteRespuestaDto>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano);

    Task<PacienteRespuestaDto?> ObtenerPorIdAsync(long id);

    Task<PacienteRespuestaDto> CrearAsync(PacienteCrearDto dto);

    Task<bool> ActualizarAsync(
        long id, PacienteActualizarDto dto);

    Task<bool> EliminarAsync(long id, string motivo);

    Task<bool> ReactivarAsync(long id);
}
