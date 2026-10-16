using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface ISeudonimoRepositorio
    {
        Task<List<Seudonimo>> ObtenerTodosAsync(bool incluirInactivos, int pagina, int tamano);

        Task<Seudonimo?> ObtenerPorIdAsync(Guid id);
        Task<Seudonimo?> ObtenerPorPacienteAsync(long pacienteId);

        Task<bool> ExistePacienteActivoAsync(long pacienteId);

        Task<string?> ObtenerEstadoConsentimientoAsync(long pacienteId, int finalidadId);

        Task<List<long>> ObtenerPacientesSinSeudonimoAsync(int finalidadId);

        // Pacientes activos con consentimiento ACEPTADO para la finalidad y con seudónimo activo.
        Task<int> ContarPacientesConSeudonimoActivoAsync(int finalidadId);

        Task<Seudonimo> CrearAsync(Seudonimo seudonimo);

        Task CrearVariosAsync(List<Seudonimo> seudonimos);

        Task GuardarCambiosAsync();
    }
}
