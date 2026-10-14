
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IHistoriaRepositorio
{
    Task<List<Historia>> ObtenerTodosAsync(
        string? estado, int pagina, int tamano);

    Task<Historia?> ObtenerPorIdAsync(long id);

    Task<Historia?> ObtenerPorPacienteAsync(long pacienteId);

    Task<bool> PacienteActivoAsync(long pacienteId);

    Task<bool> UsuarioActivoAsync(long usuarioId);

    Task<Historia> CrearAsync(Historia historia);

    Task GuardarCambiosAsync();

    void EstablecerVersionOriginal(
        Historia historia, byte[] version);
}

