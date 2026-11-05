using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface IIncidenteRepositorio
    {
        Task<List<Incidente>> ObtenerTodosAsync(string? estado, int pagina, int tamano);

        Task<Incidente?> ObtenerPorIdAsync(long id);

        Task<List<AccionIncidente>> ObtenerAccionesAsync(long incidenteId);

        Task<bool> ExisteUsuarioActivoAsync(long usuarioId);

        Task<Incidente> CrearAsync(Incidente incidente, AccionIncidente accion);

        Task ActualizarAsync(Incidente incidente, byte[] versionLeida);

        Task CambiarEstadoAsync(Incidente incidente, AccionIncidente accion, byte[] versionLeida);

        Task<AccionIncidente> CrearAccionAsync(AccionIncidente accion);
    }
}
