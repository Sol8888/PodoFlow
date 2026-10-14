using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface ITratamientoCategoriaRepositorio
    {
        Task<List<TratamientoCategoria>> ObtenerTodosAsync(
            int? tratamientoId, int? categoriaId, bool incluirInactivos, int pagina, int tamano);

        Task<TratamientoCategoria?> ObtenerAsync(int tratamientoId, int categoriaId);

        Task<TratamientoCategoria> CrearAsync(TratamientoCategoria asignacion);

        Task GuardarCambiosAsync();
    }
}
