
using PodoFlow.Api.DTOs.Usuario;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IUsuarioServicio
{
    Task<List<UsuarioRespuestaDto>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano);

    Task<UsuarioRespuestaDto?> ObtenerPorIdAsync(long id);
    Task<UsuarioRespuestaDto> CrearAsync(UsuarioCrearDto dto);

    Task<bool> ActualizarAsync(
        long id, UsuarioActualizarDto dto);

    Task<bool> EliminarAsync(long id);
    Task<bool> ReactivarAsync(long id);
}
