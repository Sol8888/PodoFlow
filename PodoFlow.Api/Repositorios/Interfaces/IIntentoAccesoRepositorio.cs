
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IIntentoAccesoRepositorio
{
    Task<IntentoAcceso> CrearAsync(IntentoAcceso intento);

    Task<IntentoAcceso?> ObtenerPorIdAsync(long id);

    Task<List<IntentoAcceso>> ObtenerTodosAsync(
        long? usuarioId,
        bool? exitoso,
        int pagina,
        int tamano);
}
