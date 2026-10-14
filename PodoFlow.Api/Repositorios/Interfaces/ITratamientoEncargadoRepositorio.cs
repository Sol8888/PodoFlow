using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface ITratamientoEncargadoRepositorio
    {
        Task<List<TratamientoEncargado>> ObtenerTodosAsync(
            int? tratamientoId, int? encargadoId, bool incluirInactivos, int pagina, int tamano);

        Task<TratamientoEncargado?> ObtenerAsync(int tratamientoId, int encargadoId);

        Task<TratamientoEncargado> CrearAsync(TratamientoEncargado asignacion);

        Task GuardarCambiosAsync();
    }
}
