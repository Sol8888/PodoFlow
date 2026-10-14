
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IDatoContactoRepositorio
{
    Task<List<DatoContacto>> ListarAsync(
        long? pacienteId, bool incluirInactivos,
        int pagina, int tamano);

    Task<DatoContacto?> ObtenerAsync(long id);

    Task<DatoContacto?> BuscarAsync(
        long pacienteId, string tipo, string valor);

    Task<bool> PacienteActivoAsync(long id);

    Task<DatoContacto> CrearAsync(DatoContacto contacto);

    Task GuardarAsync();
}
