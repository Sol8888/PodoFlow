using PodoFlow.Api.DTOs.Documento;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface IDocumentoServicio
    {
        Task<List<DocumentoRespuestaDto>> ObtenerTodosAsync(string? tipo, bool incluirInactivos, int pagina, int tamano);

        Task<DocumentoRespuestaDto?> ObtenerPorIdAsync(long id);

        Task<DocumentoRespuestaDto> CrearAsync(DocumentoCrearDto dto);

        Task<bool> ActualizarAsync(long id, DocumentoActualizarDto dto);

        Task<bool> EliminarAsync(long id);

        Task<bool> ReactivarAsync(long id);
    }
}
