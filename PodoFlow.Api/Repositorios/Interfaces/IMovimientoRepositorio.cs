
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IMovimientoRepositorio
{
    Task<List<Movimiento>> ObtenerTodosAsync(
        int? insumoId,
        long? atencionId,
        string? tipo,
        int pagina,
        int tamano);

    Task<Movimiento?> ObtenerPorIdAsync(long id);

    Task<Insumo?> ObtenerInsumoAsync(int id);

    Task<bool> UsuarioActivoAsync(long id);

    Task<bool> AtencionExisteAsync(long id);

    Task<int> ActualizarStockAsync(
        int insumoId,
        byte[] versionOriginal,
        decimal stockAnterior,
        decimal stockNuevo,
        DateTime fechaModificacion);

    Task AgregarAsync(Movimiento movimiento);

    Task GuardarCambiosAsync();
}
