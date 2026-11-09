using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface IOpcionComunicacionRepositorio
    {
        Task<List<OpcionComunicacion>> ObtenerTodosAsync(
            long? pacienteId, int? finalidadId, string? canal, bool incluirInactivos, int pagina, int tamano);

        Task<OpcionComunicacion?> ObtenerPorIdAsync(long id);

        Task<OpcionComunicacion?> ObtenerPorClaveAsync(long pacienteId, string canal, int finalidadId);

        Task<Consentimiento?> ObtenerConsentimientoAsync(long pacienteId, int finalidadId);

        Task<bool> ExistePacienteActivoAsync(long pacienteId);

        Task<OpcionComunicacion> CrearAsync(OpcionComunicacion opcion);

        Task GuardarCambiosAsync();
    }
}
