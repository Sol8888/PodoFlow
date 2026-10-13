
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IHorarioAtencionRepositorio
{
    Task<List<HorarioAtencion>> ObtenerTodosAsync(
        bool incluirInactivos,
        int pagina,
        int tamano);

    Task<HorarioAtencion?> ObtenerPorIdAsync(int id);

    Task<bool> UsuarioActivoAsync(long usuarioId);

    Task<bool> ExisteSolapamientoAsync(
        HorarioAtencion horario,
        int? excluirId = null);

    Task<HorarioAtencion> CrearAsync(
        HorarioAtencion horario);

    Task GuardarCambiosAsync();
}
