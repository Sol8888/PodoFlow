
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface ICambioCitaRepositorio
{
    Task<CambioCita> CrearAsync(CambioCita cambio);

    Task<CambioCita?> ObtenerPorIdAsync(long id);

    Task<List<CambioCita>> ObtenerTodosAsync(
        long? citaId,
        int pagina,
        int tamano);
}
