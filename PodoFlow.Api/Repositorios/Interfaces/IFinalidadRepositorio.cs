using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface IFinalidadRepositorio
    {
        Task<List<Finalidad>> ObtenerTodosAsync(bool incluirInactivos, int pagina, int tamano);
        Task<Finalidad?> ObtenerPorIdAsync(int id);
        Task<Finalidad?> ObtenerPorCodigoAsync(string codigo);
        Task<Finalidad> CrearAsync(Finalidad finalidad);
        Task GuardarCambiosAsync();
    }
} 
