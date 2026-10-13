
using PodoFlow.Api.DTOs.PacienteUsuario;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IPacienteUsuarioServicio
{
    Task<List<PacienteUsuarioRespuestaDto>> ListarAsync(
        bool incluirInactivos, int pagina, int tamano);

    Task<PacienteUsuarioRespuestaDto?> ObtenerAsync(
        long pacienteId, long usuarioId);

    Task<PacienteUsuarioRespuestaDto> CrearAsync(
        PacienteUsuarioCrearDto dto);

    Task<bool> ActualizarAsync(
        long pacienteId, long usuarioId,
        PacienteUsuarioActualizarDto dto);

    Task<bool> EliminarAsync(long pacienteId, long usuarioId);
    Task<bool> ReactivarAsync(long pacienteId, long usuarioId);
}
