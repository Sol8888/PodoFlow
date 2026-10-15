
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IInsumoRepositorio
{
    Task<List<Insumo>> ObtenerTodosAsync(
        bool incluirInactivos,
        int pagina,
        int tamano);

    Task<Insumo?> ObtenerPorIdAsync(int id);

    Task<List<Insumo>> BuscarIdenticosAsync(
        string nombre,
        string categoria,
        string unidad);

    Task<int> ContarAsync(bool incluirInactivos);

    Task AgregarAsync(Insumo insumo);

    Task GuardarCambiosAsync();

    void EstablecerVersionOriginal(
        Insumo insumo,
        byte[] version);
}
