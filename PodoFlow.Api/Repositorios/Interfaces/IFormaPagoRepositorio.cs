
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IFormaPagoRepositorio
{
    Task<List<FormaPago>> ObtenerTodosAsync(
        bool incluirInactivos,
        int pagina,
        int tamano);

    Task<FormaPago?> ObtenerPorIdAsync(int id);

    Task<FormaPago?> ObtenerPorCodigoAsync(string codigo);

    Task AgregarAsync(FormaPago formaPago);

    Task GuardarCambiosAsync();
}
