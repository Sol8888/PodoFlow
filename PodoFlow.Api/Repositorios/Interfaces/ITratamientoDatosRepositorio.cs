using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface ITratamientoDatosRepositorio
    {
        Task<List<TratamientoDatos>> ObtenerTodosAsync(bool incluirInactivos, int pagina, int tamano);

        Task<TratamientoDatos?> ObtenerPorIdAsync(int id);

        Task<TratamientoDatos?> ObtenerPorCodigoAsync(string codigo);

        Task<bool> ExisteUsuarioActivoAsync(long usuarioId);

        Task<TratamientoDatos> CrearAsync(TratamientoDatos tratamiento);

        Task GuardarCambiosAsync();
    }
}
