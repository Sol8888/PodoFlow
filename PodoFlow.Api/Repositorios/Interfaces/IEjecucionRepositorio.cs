using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface IEjecucionRepositorio
    {
        Task<List<Ejecucion>> ObtenerTodosAsync(string? estado, int pagina, int tamano);

        Task<Ejecucion?> ObtenerPorIdAsync(long id);

        Task<List<DatoAnalitico>> ObtenerDatosAsync(long ejecucionId, int pagina, int tamano);

        Task<int> ContarDatosAsync(long ejecucionId);

        Task<List<Guid>> ObtenerSeudonimosCargadosAsync(long ejecucionId, List<Guid> seudonimos);

        Task<List<Guid>> ObtenerSeudonimosAutorizadosAsync(List<Guid> seudonimos, int finalidadId);

        Task<List<Recomendacion>> ObtenerRecomendacionesAsync(long ejecucionId);

        Task<bool> ExisteUsuarioActivoAsync(long usuarioId);

        Task<Ejecucion> CrearAsync(Ejecucion ejecucion);

        Task CrearDatosAsync(List<DatoAnalitico> datos);

        Task<Recomendacion> CrearRecomendacionAsync(Recomendacion recomendacion);

        Task GuardarCambiosAsync();
    }
}
