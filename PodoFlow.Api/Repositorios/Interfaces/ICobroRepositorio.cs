
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface ICobroRepositorio
{
    Task<List<Cobro>> ObtenerTodosAsync(
        long? prestacionId,
        string? estado,
        int pagina,
        int tamano);

    Task<Cobro?> ObtenerPorIdAsync(long id);

    Task<Cobro?> ObtenerPorIdSinSeguimientoAsync(long id);

    Task<Prestacion?> BloquearPrestacionAsync(long id);

    Task<Prestacion?> ObtenerPrestacionAsync(long id);

    Task<FormaPago?> ObtenerFormaPagoAsync(int id);

    Task<bool> UsuarioActivoAsync(long id);

    Task<decimal> TotalCobradoAsync(long prestacionId);

    Task AgregarAsync(Cobro cobro);

    Task GuardarCambiosAsync();
}
