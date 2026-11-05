using PodoFlow.Api.DTOs.Notificacion;

namespace PodoFlow.Api.Integraciones.Interfaces
{
    public interface IProveedorCorreo
    {
        Task<ResultadoProveedorDto> EnviarAsync(string destino, string nombreDestino, string asunto, string contenidoHtml);
    }
}
