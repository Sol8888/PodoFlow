
using PodoFlow.Api.DTOs.UsuarioRol;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IUsuarioRolServicio
{
    Task<List<UsuarioRolRespuestaDto>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano);

    Task<UsuarioRolRespuestaDto?> ObtenerPorIdAsync(
        long usuarioId, int rolId);

    Task<UsuarioRolRespuestaDto> CrearAsync(
        UsuarioRolCrearDto dto);

    Task<bool> ActualizarAsync(
        long usuarioId,
        int rolId,
        UsuarioRolActualizarDto dto);

    Task<bool> EliminarAsync(long usuarioId, int rolId);

    Task<bool> ReactivarAsync(long usuarioId, int rolId);
}
