using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface IConsentimientoRepositorio
    {
        Task<List<Consentimiento>> ObtenerTodosAsync(int pagina, int tamano);

        Task<List<Consentimiento>> ObtenerPorPacienteAsync(long pacienteId);

        Task<Consentimiento?> ObtenerPorIdAsync(long id);

        Task<bool> ExistePorPacienteYFinalidadAsync(long pacienteId, int finalidadId);

        Task<bool> ExistePacienteActivoAsync(long pacienteId);

        Task<Consentimiento> CrearAsync(Consentimiento consentimiento, EventoConsentimiento evento);

        Task ActualizarAsync(Consentimiento consentimiento, byte[] versionLeida);

        Task CambiarEstadoAsync(Consentimiento consentimiento, EventoConsentimiento evento, byte[] versionLeida);
        Task<List<EventoConsentimiento>> ObtenerEventosAsync(long consentimientoId);
    }
}
