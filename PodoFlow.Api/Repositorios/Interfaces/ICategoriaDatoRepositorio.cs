using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface ICategoriaDatoRepositorio
    {
        Task<List<CategoriaDato>> ObtenerTodosAsync(bool incluirInactivos, int pagina, int tamano);

        Task<CategoriaDato?> ObtenerPorIdAsync(int id);

        Task<CategoriaDato?> ObtenerPorNombreAsync(string nombre);

        Task<CategoriaDato> CrearAsync(CategoriaDato categoria);

        Task GuardarCambiosAsync();
    }
}
