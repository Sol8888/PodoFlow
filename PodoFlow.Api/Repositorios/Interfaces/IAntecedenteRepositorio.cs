
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IAntecedenteRepositorio
{
    Task<List<Antecedente>> ObtenerTodosAsync(
        long? historiaId,
        bool incluirInactivos,
        int pagina,
        int tamano);

    Task<Antecedente?> ObtenerPorIdAsync(long id);

    Task<Antecedente?> BuscarIgualAsync(
        long historiaId,
        string tipo,
        string descripcion);

    Task<Historia?> ObtenerHistoriaAsync(long historiaId);

    Task<bool> PacienteActivoAsync(long pacienteId);

    Task<bool> UsuarioActivoAsync(long usuarioId);

    Task<Antecedente> CrearAsync(Antecedente antecedente);

    Task GuardarCambiosAsync();
}
