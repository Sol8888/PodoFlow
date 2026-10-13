
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IPacienteRepositorio
{
    Task<List<Paciente>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano);

    Task<Paciente?> ObtenerPorIdAsync(long id);

    Task<Paciente?> ObtenerPorIdentificacionAsync(
        string tipo, string identificacion);

    Task<Paciente> CrearAsync(Paciente paciente);

    Task GuardarCambiosAsync();

    void EstablecerVersionOriginal(
        Paciente paciente, byte[] version);
}
