
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IEvolucionRepositorio
{
    Task<List<Evolucion>> ObtenerTodosAsync(
        long? atencionId,
        bool incluirInactivos,
        int pagina,
        int tamano);

    Task<Evolucion?> ObtenerPorIdAsync(long id);

    Task<Evolucion?> BuscarIgualAsync(
        long atencionId,
        long usuarioId,
        string descripcion);

    Task<Atencion?> ObtenerAtencionAsync(long id);

    Task<Historia?> ObtenerHistoriaAsync(long id);

    Task<bool> PacienteActivoAsync(long id);

    Task<bool> UsuarioActivoAsync(long id);

    Task<Evolucion> CrearAsync(Evolucion evolucion);

    Task GuardarCambiosAsync();
}
