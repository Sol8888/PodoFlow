
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IBloqueoAgendaRepositorio
{
    Task<List<BloqueoAgenda>> ObtenerTodosAsync(
        bool incluirInactivos,
        long? usuarioId,
        int pagina,
        int tamano);

    Task<BloqueoAgenda?> ObtenerPorIdAsync(long id);

    Task<bool> UsuarioActivoAsync(long usuarioId);

    Task<BloqueoAgenda?> BuscarIgualAsync(
        long? usuarioId,
        DateTime inicio,
        DateTime fin,
        string motivo);

    Task<bool> ExisteSolapamientoAsync(
        long? usuarioId,
        DateTime inicio,
        DateTime fin,
        long? excluirId = null);

    Task CrearAsync(BloqueoAgenda bloqueo);

    Task GuardarCambiosAsync();
}
