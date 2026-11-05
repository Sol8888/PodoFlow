using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface IDocumentoRepositorio
    {
        Task<List<Documento>> ObtenerTodosAsync(string? tipo, bool incluirInactivos, int pagina, int tamano);

        Task<Documento?> ObtenerPorIdAsync(long id);

        // Otro documento con el mismo tipo, nombre y versión (activo o no), excluyendo un id.
        Task<Documento?> ObtenerDuplicadoAsync(string tipo, string nombre, string? version, long? excluirId);

        Task<bool> ExisteUsuarioActivoAsync(long usuarioId);

        Task<Documento> CrearAsync(Documento documento);

        Task GuardarCambiosAsync();
    }
}
