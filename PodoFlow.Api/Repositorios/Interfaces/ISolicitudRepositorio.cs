using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface ISolicitudRepositorio
    {
        Task<List<Solicitud>> ObtenerTodosAsync(int pagina, int tamano);

        Task<List<Solicitud>> ObtenerPorPacienteAsync(long pacienteId);

        Task<Solicitud?> ObtenerPorIdAsync(long id);

        Task<List<AccionSolicitud>> ObtenerAccionesAsync(long solicitudId);

        Task<bool> ExistePacienteActivoAsync(long pacienteId);

        Task<bool> ExisteUsuarioActivoAsync(long usuarioId);

        Task<bool> ExisteExportacionEntregadaAsync(long solicitudId);

        Task<(string TipoIdentificacion, string Identificacion, string Nombre)?> ObtenerIdentidadPacienteAsync(long pacienteId);

        Task<Solicitud> CrearAsync(Solicitud solicitud, AccionSolicitud? accion);

        Task ActualizarAsync(Solicitud solicitud, byte[] versionLeida);

        Task CambiarEstadoAsync(Solicitud solicitud, AccionSolicitud accion, byte[] versionLeida);
    }
}