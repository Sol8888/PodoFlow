
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IPacienteUsuarioRepositorio
{
    Task<List<PacienteUsuario>> ListarAsync(
        bool incluirInactivos, int pagina, int tamano);

    Task<PacienteUsuario?> ObtenerAsync(
        long pacienteId, long usuarioId);

    Task<PacienteUsuario?> PorPacienteAsync(long pacienteId);
    Task<PacienteUsuario?> PorUsuarioAsync(long usuarioId);

    Task<bool> PacienteActivoAsync(long id);
    Task<bool> UsuarioActivoAsync(long id);

    Task CrearAsync(PacienteUsuario relacion);
    Task GuardarAsync();
}
